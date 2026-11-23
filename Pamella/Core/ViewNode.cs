/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Blindness.Bind;
using Blindness.Core;

namespace Pamella;

/// <summary>
/// The base class of all generated views.
/// </summary>
public abstract partial class ViewNode : Node
{
    static readonly ConcurrentDictionary<(Type, string), (nint Pointer, bool ReturnsTask)> hidden = new();

    [ThreadStatic] internal static ViewNode Owner;
    [ThreadStatic] internal static ViewNode Behaving;
    [ThreadStatic] static ViewNode painting;

    readonly List<Action> cleanups = [];
    readonly HashSet<ViewNode> children = [];
    readonly Dictionary<string, int> slotCounters = [];
    CancellationTokenSource loadSource;
    IBox<float, float> localX, localY;

    internal PaintList PaintOps { get; } = new();
    internal ViewNode ParentNode { get; set; }
    internal ContextNode Context { get; private set; }
    internal bool Mounted { get; private set; }
    internal bool Painted { get; private set; }
    internal ViewEvents Events { get; private set; }
    internal Layer ViewLayer { get; set; }
    internal int ComposedAt { get; set; } = -1;
    internal float ParentOffsetX { get; set; }
    internal float ParentOffsetY { get; set; }

    internal float LocalXValue => (localX ??= Binding.Get(this).Dictionary.GetOrCreate<float>("LocalX")).Open();
    internal float LocalYValue => (localY ??= Binding.Get(this).Dictionary.GetOrCreate<float>("LocalY")).Open();

    internal float GlobalX => ParentNode is null
        ? LocalXValue
        : ParentNode.GlobalX + ParentOffsetX + LocalXValue;

    internal float GlobalY => ParentNode is null
        ? LocalYValue
        : ParentNode.GlobalY + ParentOffsetY + LocalYValue;

    internal CancellationToken StateToken => Context?.StateSource.Token ?? CancellationToken.None;
    internal CancellationToken LoadToken => loadSource?.Token ?? CancellationToken.None;

    protected internal virtual void CallDesign() { }
    protected internal virtual void CallBehaviour() { }
    protected internal virtual void CallReloaded() { }
    protected internal virtual Task CallState() => null;
    protected internal virtual Task CallLoad() => null;
    protected internal virtual Task CallUnload() => null;
    protected internal virtual bool HasPaint => false;
    protected internal virtual void CallPaint() { }
    protected internal virtual bool HasTick => false;
    protected internal virtual void CallTick(float dt) { }
    protected internal virtual bool HasArea => false;
    protected internal virtual Area CallArea() => default;
    protected internal virtual void WireInvalidation() { }
    protected internal virtual void CaptureEager() { }
    protected internal virtual void RestoreEager() { }

    /// <summary>
    /// Join the tasks returned by the lifecycle functions.
    /// </summary>
    protected static Task Join(List<Task> tasks)
    {
        tasks.RemoveAll(task => task is null);
        return tasks.Count switch
        {
            0 => null,
            1 => tasks[0],
            _ => Task.WhenAll(tasks)
        };
    }

    /// <summary>
    /// Call a interface function hidden by a member of the node with same name.
    /// </summary>
    protected Task InvokeHidden(Type owner, string name)
    {
        var (pointer, returnsTask) = hidden.GetOrAdd((owner, name), static key =>
        {
            var method = key.Item1.GetMethod(
                key.Item2,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                Type.EmptyTypes);
            return (method.MethodHandle.GetFunctionPointer(), method.ReturnType == typeof(Task));
        });

        if (returnsTask)
            return ((Func<Task>)Activator.CreateInstance(typeof(Func<Task>), this, pointer))();

        ((Action)Activator.CreateInstance(typeof(Action), this, pointer))();
        return null;
    }

    /// <summary>
    /// Invalidate the view when a property, or anything read by its binding, changes value.
    /// </summary>
    protected void Watch<T>(string name)
    {
        var box = Binding.Get(this).Dictionary.GetOrCreate<T>(name);
        HashSet<INotifyBox> reads = [];
        T last = default;
        Action changed = null;

        void track()
        {
            HashSet<INotifyBox> current = [];
            last = BoxTracker.Track(box.Open, current);
            foreach (var read in reads)
                if (!current.Contains(read))
                    read.Changed -= changed;
            foreach (var read in current)
                if (!reads.Contains(read))
                    read.Changed += changed;
            reads = current;
        }

        changed = () =>
        {
            var old = last;
            track();
            if (!EqualityComparer<T>.Default.Equals(old, last))
                Invalidate();
        };

        track();
        cleanups.Add(() =>
        {
            foreach (var read in reads)
                read.Changed -= changed;
        });
    }

    /// <summary>
    /// Validate a resource that will be shared and get the object to store.
    /// </summary>
    protected object Cacheable(string property, object value)
    {
        if (value is ICachedResource resource)
        {
            resource.ValidateCache($"{GetType().Name}.{property}");
            ViewRegistry.Captured?.Add(resource);
        }
        return value;
    }

    /// <summary>
    /// Get the instance of a cached resource that belongs to this view.
    /// </summary>
    protected object ForInstance(object value)
        => value is ICachedResource resource ? resource.ForOwner(this) : value;

    /// <summary>
    /// Run the Design function when the node is created.
    /// </summary>
    public override void Load()
    {
        var previous = Owner;
        Owner = this;
        try
        {
            if (!ViewRegistry.TakeModelFlag())
            {
                ViewRegistry.EnsureWarm(GetType());
                RestoreEager();
            }
            CallDesign();
        }
        finally
        {
            Owner = previous;
        }
    }

    /// <summary>
    /// Request a new Paint of this view.
    /// </summary>
    public void Invalidate()
    {
        if (!Mounted)
            return;

        if (painting is ViewNode source)
            throw new InvalidOperationException(
                $"{Name(source)}.Paint() invalidated {Name(this)}. Move the state change out of Paint.");

        Scene.MarkDirty(this);
    }

    static string Name(ViewNode view)
        => ViewRegistry.FindByConcrete(view.GetType())?.Name ?? view.GetType().Name;

    internal void AddCleanup(Action cleanup)
        => cleanups.Add(cleanup);

    internal void AddChild(ViewNode child)
    {
        if (child.ParentNode is not null && child.ParentNode != this)
            child.ParentNode.children.Remove(child);

        child.ParentNode = this;
        children.Add(child);
    }

    internal void PaintChild(View view)
    {
        var child = (ViewNode)(object)view;
        AddChild(child);

        var type = child.GetType().FullName;
        slotCounters.TryGetValue(type, out var index);
        slotCounters[type] = index + 1;

        child.EnsureReady(this, $"{type}#{index}");
        PaintOps.AddChild(child);
    }

    /// <summary>
    /// Mount this view and record its first Paint when needed.
    /// </summary>
    internal void EnsureReady(ViewNode parent, string slot)
    {
        if (!Mounted)
            Mount(parent?.EnsureContext().GetChild(slot) ?? new ContextNode(null, slot));

        if (!Painted)
            Repaint();
    }

    internal ContextNode EnsureContext()
        => Context ?? new ContextNode(null, GetType().FullName);

    internal void Mount(ContextNode context)
    {
        if (Mounted)
            return;

        var old = context.Instance;
        var reloading = old is not null && old != this;
        Context = context;
        if (reloading)
            TakeOver(old);
        context.Instance = this;

        var previous = Owner;
        var previousPainting = painting;
        Owner = this;
        painting = null;
        try
        {
            WireInvalidation();
            WatchPosition();

            Events = new ViewEvents();
            Behaving = this;
            try { CallBehaviour(); }
            finally { Behaving = null; }

            Mounted = true;
            Scene.Track(this);
            CaptureBaseline();

            if (!context.StateStarted)
                StartState();

            loadSource = new CancellationTokenSource();
            Scene.Observe(CallLoad(), this, "Load");

            if (reloading)
                CallReloaded();
        }
        finally
        {
            Owner = previous;
            painting = previousPainting;
        }

        Scene.MarkDirty(this);
    }

    internal void ReloadState()
    {
        if (Context is null)
            return;

        Context.RestartState();
        StartState();
    }

    void StartState()
    {
        Context.StateStarted = true;
        Context.StateTask = CallState();
        Scene.Observe(Context.StateTask, this, "State");
    }

    void WatchPosition()
    {
        var x = Binding.Get(this).Dictionary.GetOrCreate<float>("LocalX");
        var y = Binding.Get(this).Dictionary.GetOrCreate<float>("LocalY");
        Action moved = Scene.MarkComposition;
        ((INotifyBox)x).Changed += moved;
        ((INotifyBox)y).Changed += moved;
        cleanups.Add(() =>
        {
            ((INotifyBox)x).Changed -= moved;
            ((INotifyBox)y).Changed -= moved;
        });
    }

    /// <summary>
    /// Record again the paint list of this view.
    /// </summary>
    internal void Repaint()
    {
        PaintOps.Clear();
        slotCounters.Clear();
        Painted = true;
        Scene.MarkComposition();

        if (!HasPaint)
            return;

        var previousOwner = Owner;
        var previousPaint = PaintList.Current;
        var previousPainting = painting;
        Owner = this;
        PaintList.Current = PaintOps;
        painting = this;
        try
        {
            CallPaint();
        }
        finally
        {
            painting = previousPainting;
            PaintList.Current = previousPaint;
            Owner = previousOwner;
        }
    }

    /// <summary>
    /// Get the area of this view in local coordinates.
    /// </summary>
    internal Area MeasureArea()
    {
        if (!Mounted || !Painted)
            EnsureReady(ParentNode, GetType().FullName);

        if (!HasArea)
            return PaintOps.GetBounds();

        var previous = Owner;
        Owner = this;
        try { return CallArea(); }
        finally { Owner = previous; }
    }

    internal void NotifyResize(Area before)
    {
        if (Events?.Resize is not Action<float, float> handler)
            return;

        var after = MeasureArea();
        if (after.Width != before.Width || after.Height != before.Height)
            handler(after.Width, after.Height);
    }

    /// <summary>
    /// Release the links of this instance keeping the component alive.
    /// </summary>
    internal void Detach()
    {
        Mounted = false;
        Scene.Untrack(this);
        Events = null;
        loadSource?.Cancel();
        loadSource = null;
        foreach (var cleanup in cleanups)
            cleanup();
        cleanups.Clear();
        base.Unload();

        foreach (var child in children.ToArray())
            child.Detach();
    }

    /// <summary>
    /// End this component and its children.
    /// </summary>
    public override void Unload()
    {
        foreach (var child in children.ToArray())
            child.Unload();

        if (Mounted)
            Scene.Observe(CallUnload(), this, "Unload");

        Context?.End();
        Detach();
    }

    partial void TakeOver(ViewNode old);
    partial void CaptureBaseline();
}
