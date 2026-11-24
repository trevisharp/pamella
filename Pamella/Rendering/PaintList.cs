/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Radiance;
using Radiance.Fonts;
using Radiance.Bufferings;

namespace Pamella;

enum PaintOpKind
{
    Draw,
    Clip,
    Offset,
    Child
}

/// <summary>
/// Represents one step recorded by a Paint function.
/// </summary>
struct PaintOp
{
    public PaintOpKind Kind;
    public ShaderDefinition Shader;
    public Polygon Polygon;
    public float[] Floats;
    public object[] Textures;
    public Area Area;
    public float OriginX;
    public float OriginY;
    public ViewNode Child;
}

/// <summary>
/// Stores what a view draws, in order. Nothing here touches the GPU.
/// </summary>
sealed class PaintList
{
    [ThreadStatic] public static PaintList Current;

    public List<PaintOp> Ops { get; } = [];

    public void Clear() => Ops.Clear();

    /// <summary>
    /// Record a shader call at the origin of the view.
    /// </summary>
    public void AddDraw(ShaderDefinition shader, Polygon polygon, float[] floats, object[] textures)
        => Ops.Add(new PaintOp
        {
            Kind = PaintOpKind.Draw,
            Shader = shader,
            Polygon = polygon,
            Floats = floats,
            Textures = textures,
            Area = Geometry.Bounds(polygon)
        });

    /// <summary>
    /// Record a shader call moved inside the view, with known bounds.
    /// </summary>
    public void AddDraw(ShaderDefinition shader, Polygon polygon, float[] floats, object[] textures, float x, float y, Area bounds)
        => Ops.Add(new PaintOp
        {
            Kind = PaintOpKind.Draw,
            Shader = shader,
            Polygon = polygon,
            Floats = floats,
            Textures = textures,
            Area = bounds.Move(x, y),
            OriginX = x,
            OriginY = y
        });

    /// <summary>
    /// Record a shader call for each glyph of a text moved inside the view.
    /// </summary>
    public void AddText(ShaderDefinition shader, TextRun text, float[] floats, object[] textures, float x, float y)
    {
        foreach (var glyph in text.Glyphs)
            AddDraw(shader, glyph.Polygon, floats, textures, x + glyph.X, y + glyph.Y, glyph.Bounds);
    }

    public void AddClip(Area area)
        => Ops.Add(new PaintOp { Kind = PaintOpKind.Clip, Area = area });

    public void AddOffset(float x, float y)
        => Ops.Add(new PaintOp { Kind = PaintOpKind.Offset, Area = new(x, y, 0, 0) });

    public void AddChild(ViewNode child)
        => Ops.Add(new PaintOp { Kind = PaintOpKind.Child, Child = child });

    /// <summary>
    /// Get the region covered by the recorded steps.
    /// </summary>
    public Area GetBounds()
    {
        Area result = default;
        float offsetX = 0, offsetY = 0;
        foreach (var op in Ops)
        {
            switch (op.Kind)
            {
                case PaintOpKind.Draw:
                    result = result.Union(op.Area);
                    break;

                case PaintOpKind.Offset:
                    offsetX = op.Area.X;
                    offsetY = op.Area.Y;
                    break;

                case PaintOpKind.Child:
                    var child = op.Child.MeasureArea();
                    result = result.Union(child.Move(
                        offsetX + op.Child.LocalXValue,
                        offsetY + op.Child.LocalYValue));
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Get the region covered only by the draws of the view itself.
    /// </summary>
    public Area GetOwnBounds()
    {
        Area result = default;
        foreach (var op in Ops)
            if (op.Kind == PaintOpKind.Draw)
                result = result.Union(op.Area);
        return result;
    }
}

/// <summary>
/// Stores polygons reused by many draws.
/// </summary>
static class Geometry
{
    const int limit = 4096;

    static readonly ConditionalWeakTable<Polygon, object> bounds = [];
    static readonly Dictionary<(float, float), Polygon> rects = [];
    static readonly Dictionary<(string, float, IFont, float), TextRun> texts = [];

    public static readonly Polygon UnitRect = Polygons.Rect(0.5f, 0.5f, 0, 1, 1);
    public static readonly Polygon UnitLine = Polygons.Rect(0.5f, 0, 0, 1, 1);
    public static readonly Polygon UnitCircle = Polygons.Circle;

    /// <summary>
    /// Get a rectangle with a corner on (0, 0).
    /// </summary>
    public static Polygon Rect(float width, float height)
    {
        if (rects.Count > limit)
            rects.Clear();

        if (!rects.TryGetValue((width, height), out var polygon))
            rects[(width, height)] = polygon = Polygons.Rect(width / 2, height / 2, 0, width, height);
        return polygon;
    }

    /// <summary>
    /// Get the glyphs of a text.
    /// </summary>
    public static TextRun Text(string text, float size, IFont font, float maxWidth = float.PositiveInfinity)
    {
        if (texts.Count > limit)
            texts.Clear();

        font ??= Font.Default;
        if (!texts.TryGetValue((text, size, font, maxWidth), out var run))
            texts[(text, size, font, maxWidth)] = run = TextRun.Create(text, size, font, maxWidth);
        return run;
    }

    /// <summary>
    /// Get the region covered by a polygon.
    /// </summary>
    public static Area Bounds(Polygon polygon)
        => (Area)bounds.GetValue(polygon, static p =>
        {
            var data = p.GetBufferData();
            float left = float.MaxValue, bottom = float.MaxValue;
            float right = float.MinValue, top = float.MinValue;
            for (int i = 0; i + 1 < data.Length; i += 3)
            {
                left = MathF.Min(left, data[i]);
                right = MathF.Max(right, data[i]);
                bottom = MathF.Min(bottom, data[i + 1]);
                top = MathF.Max(top, data[i + 1]);
            }

            return data.Length == 0 ? default(Area) : new Area(left, bottom, right - left, top - bottom);
        });
}
