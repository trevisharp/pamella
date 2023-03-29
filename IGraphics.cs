/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
using System.Drawing;
using System.Drawing.Drawing2D;

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

    /// <summary>
    /// Cursor location pixels in screen.
    /// </summary>
    PointF Cursor { get; }

    /// <summary>
    /// Return true if the left button in cursor is down. 
    /// </summary>
    bool IsDown { get; }

    /// <summary>
    /// Return true if the right button in cursor is down. 
    /// </summary>
    bool IsRightDown { get; }

    void Clear(Color color);

    void DrawRectangle(RectangleF rect, Pen pen);

    void DrawRectangle(PointF loc, Size siz, Pen pen);

    void DrawRectangle(float x, float y, float width, float height, Pen pen);

    void FillRectangle(RectangleF rect, Brush brush);

    void FillRectangle(PointF loc, Size siz, Brush brush);

    void FillRectangle(float x, float y, float width, float height, Brush brush);
}