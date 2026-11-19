/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Blindness.Core;
using Radiance;
using Radiance.Bufferings;

namespace Pamella;

/// <summary>
/// Represents a rectangle of the window used as scissor.
/// </summary>
readonly record struct Scissor(int X, int Y, int Width, int Height)
{
    public Area Area => new(X, Y, Width, Height);

    public static Scissor From(Area area)
    {
        var left = (int)MathF.Floor(area.X);
        var bottom = (int)MathF.Floor(area.Y);
        var right = (int)MathF.Ceiling(area.X + area.Width);
        var top = (int)MathF.Ceiling(area.Y + area.Height);
        return new(left, bottom, Math.Max(0, right - left), Math.Max(0, top - bottom));
    }
}

/// <summary>
/// Represents the region where a view receives input.
/// </summary>
readonly record struct HitRegion(ViewNode View, Area Area, Scissor? Scissor);

/// <summary>
/// Represents the draws that share a shader, a polygon and a scissor.
/// </summary>
sealed class Batch(CanonicalShader shader, Polygon polygon, Scissor? scissor, object[] textures, int stride)
{
    float[] data = new float[stride * 8];

    public ShaderDefinition Definition { get; init; }
    public CanonicalShader Shader => shader;
    public Polygon Polygon => polygon;
    public Scissor? Scissor => scissor;
    public object[] Textures => textures;
    public int Stride => stride;
    public float[] Data => data;
    public int Count { get; private set; }
    public Area Bounds { get; private set; }

    public bool Matches(CanonicalShader otherShader, Polygon otherPolygon, Scissor? otherScissor, object[] otherTextures)
    {
        if (shader != otherShader || polygon != otherPolygon || scissor != otherScissor)
            return false;

        for (int i = 0; i < textures.Length; i++)
            if (textures[i] != otherTextures[i])
                return false;

        return true;
    }

    public void Add(float[] floats, float x, float y, float order, Area area)
    {
        if ((Count + 1) * stride > data.Length)
            Array.Resize(ref data, data.Length * 2);

        var start = Count * stride;
        Array.Copy(floats, 0, data, start, floats.Length);
        data[start + floats.Length] = x;
        data[start + floats.Length + 1] = y;
        data[start + floats.Length + 2] = order;
        Bounds = Count == 0 ? area : Bounds.Union(area);
        Count++;
    }
}

/// <summary>
/// Stores on GPU buffers the instances of a batch.
/// </summary>
sealed class BatchStore(CanonicalShader shader, Polygon polygon, object[] textures, bool[] textureSlots)
{
    const float hiddenDepth = 1_000_000f;

    BufferData[] columns;
    object[] args;
    int capacity;
    int used;

    public int IdleFrames { get; set; }

    public void Upload(Batch batch, float depthScale)
    {
        Reserve(batch.Count);

        var data = batch.Data;
        var stride = batch.Stride;
        for (int column = 0; column < stride; column++)
        {
            var target = columns[column];
            var isDepth = column == stride - 1;
            for (int i = 0; i < batch.Count; i++)
            {
                var value = data[i * stride + column];
                if (isDepth)
                    value *= depthScale;
                if (target[i] != value)
                    target[i] = value;
            }
        }

        var depth = columns[stride - 1];
        for (int i = batch.Count; i < used; i++)
            depth[i] = hiddenDepth;
        used = batch.Count;
    }

    public void Draw()
        => shader.Render.TryInvoke(null, args, out _);

    public void Dispose()
    {
        if (columns is null)
            return;

        foreach (var column in columns)
            column.Dispose();
    }

    void Reserve(int count)
    {
        if (count <= capacity)
            return;

        Dispose();
        capacity = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)Math.Max(count, 1));
        used = 0;

        var floatCount = textureSlots.Count(isTexture => !isTexture) + 3;
        columns = new BufferData[floatCount];
        for (int i = 0; i < floatCount; i++)
        {
            var isDepth = i == floatCount - 1;
            columns[i] = Buffers.Create(capacity, _ => isDepth ? hiddenDepth : 0f);
        }

        args = new object[textureSlots.Length + 4];
        args[0] = polygon * capacity;
        int floatIndex = 0, textureIndex = 0;
        for (int i = 0; i < textureSlots.Length; i++)
            args[i + 1] = textureSlots[i] ? textures[textureIndex++] : columns[floatIndex++];
        for (int i = 0; i < 3; i++)
            args[textureSlots.Length + 1 + i] = columns[floatIndex++];
    }
}

/// <summary>
/// Runs the frames of the application: Tick, Paint, composition and draw.
/// </summary>
public static class Scene
{
    const int searchLimit = 128;
    const int maxPaintRounds = 8;
    const int idleLimit = 240;

    readonly record struct StoreKey(CanonicalShader Shader, Polygon Polygon, int Index, string Textures);
    readonly record struct Deferred(ViewNode View, float X, float Y);

    static readonly HashSet<ViewNode> dirty = [];
    static readonly HashSet<ViewNode> mounted = [];
    static readonly List<ViewNode> ticking = [];
    static readonly List<Batch> batches = [];
    static readonly List<(BatchStore Store, Scissor? Scissor)> draws = [];
    static readonly Dictionary<StoreKey, BatchStore> stores = [];
    static readonly List<HitRegion> regions = [];
    static readonly List<Deferred>[] layers = [ [], [], [], [], [], ];
    static readonly Stopwatch clock = Stopwatch.StartNew();

    static readonly Stack<(ViewNode View, ContextNode Context)> previousRoots = [];

    static ViewNode root;
    static ContextNode rootContext = new(null, "root");
    static bool compositionDirty = true;
    static volatile bool eventsPending;
    static double lastTick;
    static int order;
    static Timer tickTimer;

    /// <summary>
    /// Get the number of draw calls of the last composition.
    /// </summary>
    public static int DrawCalls => draws.Count;

    /// <summary>
    /// Get the number of different shader programs in use.
    /// </summary>
    public static int ShaderCount => CanonicalShader.Count;

    /// <summary>
    /// Get the number of times a Paint function ran.
    /// </summary>
    public static int PaintCount { get; private set; }

    internal static ViewNode Root => root;
    internal static ContextNode RootContext => rootContext;
    internal static int CompositionId { get; private set; }
    internal static IReadOnlyList<HitRegion> Regions => regions;

    internal static void PushRoot(ViewNode view)
    {
        if (root is not null)
            previousRoots.Push((root, rootContext));

        root = view;
        rootContext = new ContextNode(null, "root");
        MarkComposition();
    }

    internal static void ReplaceRoot(ViewNode view)
    {
        root = view;
        MarkComposition();
    }

    internal static void PopRoot()
    {
        if (previousRoots.Count == 0)
            return;

        root.Unload();
        (root, rootContext) = previousRoots.Pop();
        MarkComposition();
    }

    internal static void Close()
    {
        root?.Unload();
        tickTimer?.Dispose();
        tickTimer = null;
    }

    internal static void Initialize()
        => Node.OnEventsPending += () =>
        {
            eventsPending = true;
            RequestFrame();
        };

    internal static void RequestFrame()
    {
        if (Window.IsOpen)
            Window.Invalidate();
    }

    internal static void MarkDirty(ViewNode view)
    {
        lock (dirty)
            dirty.Add(view);
        RequestFrame();
    }

    internal static void MarkComposition()
    {
        compositionDirty = true;
        RequestFrame();
    }

    internal static void Track(ViewNode view)
    {
        mounted.Add(view);
        if (!view.HasTick)
            return;

        ticking.Add(view);
        UpdateTimer();
    }

    internal static void Untrack(ViewNode view)
    {
        mounted.Remove(view);
        lock (dirty)
            dirty.Remove(view);
        if (ticking.Remove(view))
            UpdateTimer();
    }

    static void UpdateTimer()
    {
        if (ticking.Count == 0)
        {
            tickTimer?.Dispose();
            tickTimer = null;
            return;
        }

        lastTick = clock.Elapsed.TotalSeconds;
        tickTimer ??= new Timer(_ => RequestFrame(), null, 0, 16);
    }

    /// <summary>
    /// Report the failure of a lifecycle task, when it has one.
    /// </summary>
    internal static void Observe(Task task, ViewNode view, string name)
    {
        task?.ContinueWith(
            t => Report($"{view.GetType().Name}.{name}", t.Exception?.GetBaseException()),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    internal static void Report(string source, Exception ex)
        => Console.Error.WriteLine($"[Pamella] {source}: {ex}");

    /// <summary>
    /// Run a complete frame. Called on the render phase of the window.
    /// </summary>
    internal static void Frame()
    {
        UiContext.Drain();
        HotReload.ApplyPending();

        if (root is not null && !root.Mounted)
            Guard(root, () => root.Mount(rootContext));

        EvaluateEvents();
        RunTicks();
        RunPaints();

        if (compositionDirty)
        {
            Compose();
            HotReload.FinishPending();
        }

        DrawAll();
    }

    static void EvaluateEvents()
    {
        if (!eventsPending)
            return;

        eventsPending = false;
        foreach (var view in mounted.ToArray())
            view.EvaluateEvents();
    }

    static void RunTicks()
    {
        if (ticking.Count == 0)
            return;

        var now = clock.Elapsed.TotalSeconds;
        var dt = (float)(now - lastTick);
        lastTick = now;
        foreach (var view in ticking.ToArray())
            Guard(view, () => view.CallTick(dt));
    }

    internal static void Guard(ViewNode view, Action action)
    {
        var previous = ViewNode.Owner;
        ViewNode.Owner = view;
        try { action(); }
        catch (Exception ex) { Report(view.GetType().Name, ex); }
        finally { ViewNode.Owner = previous; }
    }

    static void RunPaints()
    {
        for (int round = 0; round < maxPaintRounds; round++)
        {
            ViewNode[] views;
            lock (dirty)
            {
                if (dirty.Count == 0)
                    return;

                views = [ ..dirty ];
                dirty.Clear();
            }

            foreach (var view in views)
            {
                if (!view.Mounted)
                    continue;

                PaintCount++;
                Guard(view, () =>
                {
                    var before = view.Events?.Resize is null ? default : view.MeasureArea();
                    view.Repaint();
                    view.NotifyResize(before);
                });
            }
        }

        RequestFrame();
    }

    /// <summary>
    /// Turn the paint lists into ordered batches.
    /// </summary>
    static void Compose()
    {
        compositionDirty = false;
        CompositionId++;
        batches.Clear();
        regions.Clear();
        order = 0;

        if (root is not null)
        {
            if (root.Mounted && !root.Painted)
                Guard(root, root.Repaint);
            Emit(root, root.LocalXValue, root.LocalYValue, null);

            foreach (var layer in layers)
            {
                for (int i = 0; i < layer.Count; i++)
                    Emit(layer[i].View, layer[i].X, layer[i].Y, null);
                layer.Clear();
            }
        }

        Upload();
    }

    static void Emit(ViewNode view, float x, float y, Scissor? scissor)
    {
        view.ComposedAt = CompositionId;
        float offsetX = 0, offsetY = 0;
        var own = view.HasArea ? view.MeasureArea() : view.PaintOps.GetOwnBounds();
        regions.Add(new HitRegion(view, own.Move(x, y), scissor));

        foreach (var op in view.PaintOps.Ops)
        {
            switch (op.Kind)
            {
                case PaintOpKind.Draw:
                    AddInstance(op, x, y, scissor);
                    break;

                case PaintOpKind.Clip:
                    var area = Scissor.From(op.Area.Move(x, y)).Area;
                    if (scissor is Scissor current)
                        area = area.Intersect(current.Area);
                    scissor = Scissor.From(area);
                    break;

                case PaintOpKind.Offset:
                    offsetX = op.Area.X;
                    offsetY = op.Area.Y;
                    break;

                case PaintOpKind.Child:
                    var child = op.Child;
                    child.ParentOffsetX = offsetX;
                    child.ParentOffsetY = offsetY;
                    var childX = x + offsetX + child.LocalXValue;
                    var childY = y + offsetY + child.LocalYValue;
                    if (child.ViewLayer > view.ViewLayer)
                        layers[(int)child.ViewLayer].Add(new Deferred(child, childX, childY));
                    else
                    {
                        child.ViewLayer = view.ViewLayer;
                        Emit(child, childX, childY, scissor);
                    }
                    break;
            }
        }
    }

    static void AddInstance(in PaintOp op, float x, float y, Scissor? scissor)
    {
        var area = op.Area.Move(x, y);
        if (scissor is Scissor limit)
        {
            area = area.Intersect(limit.Area);
            if (area.IsEmpty)
                return;
        }

        var shader = op.Shader.Resolve(op.Polygon, op.Textures);
        var batch = FindBatch(shader, op.Polygon, scissor, op.Textures, area);
        if (batch is null)
        {
            batch = new Batch(shader, op.Polygon, scissor, op.Textures, op.Floats.Length + 3)
            {
                Definition = op.Shader
            };
            batches.Add(batch);
        }

        batch.Add(op.Floats, x + op.OriginX, y + op.OriginY, ++order, area);
    }

    /// <summary>
    /// Find a batch that can receive a draw without passing over anything it overlaps.
    /// </summary>
    static Batch FindBatch(CanonicalShader shader, Polygon polygon, Scissor? scissor, object[] textures, Area area)
    {
        var last = Math.Max(0, batches.Count - searchLimit);
        for (int i = batches.Count - 1; i >= last; i--)
        {
            var batch = batches[i];
            if (batch.Matches(shader, polygon, scissor, textures))
                return batch;

            if (batch.Bounds.Overlaps(area))
                return null;
        }

        return null;
    }

    static void Upload()
    {
        draws.Clear();
        foreach (var store in stores.Values)
            store.IdleFrames++;

        var depthScale = 1000f / (order + 1);
        var usage = new Dictionary<(CanonicalShader, Polygon, string), int>();
        foreach (var batch in batches)
        {
            var textures = string.Join(',', batch.Textures.Select(t => t.GetHashCode()));
            usage.TryGetValue((batch.Shader, batch.Polygon, textures), out var index);
            usage[(batch.Shader, batch.Polygon, textures)] = index + 1;

            var key = new StoreKey(batch.Shader, batch.Polygon, index, textures);
            if (!stores.TryGetValue(key, out var store))
                stores[key] = store = new BatchStore(
                    batch.Shader, batch.Polygon, batch.Textures, batch.Definition.TextureSlots);

            store.IdleFrames = 0;
            store.Upload(batch, depthScale);
            draws.Add((store, batch.Scissor));
        }

        foreach (var (key, store) in stores.Where(s => s.Value.IdleFrames > idleLimit).ToArray())
        {
            store.Dispose();
            stores.Remove(key);
        }
    }

    static void DrawAll()
    {
        foreach (var (store, scissor) in draws)
        {
            if (scissor is Scissor area)
            {
                Window.ScissorTest = true;
                Window.ScissorPoint = (area.X, area.Y);
                Window.ScissorSize = (area.Width, area.Height);
            }
            else Window.ScissorTest = false;

            store.Draw();
        }

        Window.ScissorTest = false;
    }

    /// <summary>
    /// Forget the batches of the code that was replaced by a hot reload.
    /// </summary>
    internal static void ResetShaders()
    {
        foreach (var store in stores.Values)
            store.Dispose();
        stores.Clear();
        draws.Clear();
        batches.Clear();
        ShaderDefinition.Clear();
        CanonicalShader.Clear();
        compositionDirty = true;
    }
}
