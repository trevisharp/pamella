/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;

namespace Pamella;

/// <summary>
/// Represents a rectangular region.
/// </summary>
public readonly record struct Area(float X, float Y, float Width, float Height)
{
    /// <summary>
    /// Get if a point is inside this area.
    /// </summary>
    public bool Contains(float x, float y)
        => x >= X && y >= Y && x <= X + Width && y <= Y + Height;

    /// <summary>
    /// Get if this area has no size.
    /// </summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    /// Get this area moved by a offset.
    /// </summary>
    public Area Move(float x, float y)
        => new(X + x, Y + y, Width, Height);

    /// <summary>
    /// Get the smallest area that contains this area and another.
    /// </summary>
    public Area Union(Area other)
    {
        if (IsEmpty)
            return other;

        if (other.IsEmpty)
            return this;

        var left = MathF.Min(X, other.X);
        var bottom = MathF.Min(Y, other.Y);
        var right = MathF.Max(X + Width, other.X + other.Width);
        var top = MathF.Max(Y + Height, other.Y + other.Height);
        return new(left, bottom, right - left, top - bottom);
    }

    /// <summary>
    /// Get the region shared by this area and another.
    /// </summary>
    public Area Intersect(Area other)
    {
        var left = MathF.Max(X, other.X);
        var bottom = MathF.Max(Y, other.Y);
        var right = MathF.Min(X + Width, other.X + other.Width);
        var top = MathF.Min(Y + Height, other.Y + other.Height);
        return new(left, bottom, MathF.Max(0, right - left), MathF.Max(0, top - bottom));
    }

    /// <summary>
    /// Get if this area and another share any region.
    /// </summary>
    public bool Overlaps(Area other)
        => X < other.X + other.Width && other.X < X + Width
            && Y < other.Y + other.Height && other.Y < Y + Height;
}

/// <summary>
/// Represents the depth groups of the views.
/// </summary>
public enum Layer
{
    Content,
    Overlay,
    Modal,
    Drag,
    Debug
}
