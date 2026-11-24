/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;

using Radiance;
using Radiance.Fonts;
using Radiance.Bufferings;
using Radiance.Primitives;
using static Radiance.Utils;

namespace Pamella;

/// <summary>
/// Records ready drawing operations on the paint list of the current view.
/// </summary>
public sealed class Graphics
{
    internal static readonly Graphics Current = new();

    static readonly object[] noTextures = [];

    static ShaderDefinition solid, box, ellipse, line, image;

    static ShaderDefinition Solid => solid ??= ShaderDefinition.For((Action<vec4>)((vec4 c) =>
    {
        color = c;
        fill();
    }));

    static ShaderDefinition Box => box ??= ShaderDefinition.For((Action<vec4, val, val>)((vec4 c, val w, val h) =>
    {
        var p = pos;
        pos = vec(p.x * w, p.y * h, p.z);
        color = c;
        fill();
    }));

    static ShaderDefinition Ellipse => ellipse ??= ShaderDefinition.For((Action<vec4, val, val>)((vec4 c, val rx, val ry) =>
    {
        var p = pos;
        pos = vec(p.x * rx, p.y * ry, p.z);
        color = c;
        fill();
    }));

    static ShaderDefinition Line => line ??= ShaderDefinition.For((Action<vec4, val, val, val>)((vec4 c, val dx, val dy, val width) =>
    {
        var p = pos;
        var size = sqrt(dx * dx + dy * dy);
        var nx = 0 - dy / size;
        var ny = dx / size;
        pos = vec(p.x * dx + p.y * width * nx, p.x * dy + p.y * width * ny, p.z);
        color = c;
        fill();
    }));

    static ShaderDefinition Picture => image ??= ShaderDefinition.For((Action<img, val, val>)((img source, val w, val h) =>
    {
        var p = pos;
        var u = p.x;
        var v = p.y;
        pos = vec(u * w, v * h, p.z);
        color = texture(source, u, 1 - v);
        fill();
    }));

    static PaintList List => PaintList.Current
        ?? throw new InvalidOperationException("Graphics can only be used inside Paint.");

    Graphics() { }

    /// <summary>
    /// Fill a rectangle.
    /// </summary>
    public void FillRectangle(Vec4 color, float x, float y, float width, float height)
        => List.AddDraw(Box, Geometry.UnitRect,
            [ color.X, color.Y, color.Z, color.W, width, height ],
            noTextures, x, y, new Area(0, 0, width, height));

    /// <summary>
    /// Fill the ellipse inside a rectangle.
    /// </summary>
    public void FillEllipse(Vec4 color, float x, float y, float width, float height)
        => List.AddDraw(Ellipse, Geometry.UnitCircle,
            [ color.X, color.Y, color.Z, color.W, width / 2, height / 2 ],
            noTextures, x + width / 2, y + height / 2,
            new Area(-width / 2, -height / 2, width, height));

    /// <summary>
    /// Draw a line between two points.
    /// </summary>
    public void DrawLine(Vec4 color, float width, float x1, float y1, float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var left = MathF.Min(0, dx) - width;
        var bottom = MathF.Min(0, dy) - width;
        List.AddDraw(Line, Geometry.UnitLine,
            [ color.X, color.Y, color.Z, color.W, dx, dy, width ],
            noTextures, x1, y1,
            new Area(left, bottom, MathF.Abs(dx) + 2 * width, MathF.Abs(dy) + 2 * width));
    }

    /// <summary>
    /// Draw a text with the default font.
    /// </summary>
    public void DrawString(string text, float size, Vec4 color, float x, float y)
        => DrawString(text, null, size, color, x, y);

    /// <summary>
    /// Draw a text.
    /// </summary>
    public void DrawString(string text, IFont font, float size, Vec4 color, float x, float y)
    {
        List.AddText(Solid, Geometry.Text(text, size, font),
            [ color.X, color.Y, color.Z, color.W ],
            noTextures, x, y);
    }

    /// <summary>
    /// Draw a texture on a rectangle.
    /// </summary>
    public void DrawImage(Texture texture, float x, float y, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(texture, nameof(texture));
        List.AddDraw(Picture, Geometry.UnitRect,
            [ width, height ],
            [ texture ], x, y, new Area(0, 0, width, height));
    }
}
