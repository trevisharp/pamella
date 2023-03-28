namespace Pamella;

/// <summary>
/// A base class for all graphical interation
/// </summary>
public interface IGraphics
{
    /// <summary>
    /// A width in pixels of screen.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// A height in pixels of screen.
    /// </summary>
    int Height { get; }
}