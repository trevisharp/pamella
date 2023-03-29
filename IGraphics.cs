/* Author:  Leonardo Trevisan Silio
 * Date:    29/03/2023
 */
using System.Drawing;

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

    /// <summary>
    /// Clear a screen with a specific color.
    /// </summary>
    void Clear(Color color);

    /// <summary>
    /// Draw a rectangle in a rectangle with a specific pen.
    /// </summary>
    void DrawRectangle(RectangleF rect, Pen pen);

    /// <summary>
    /// Draw a rectangle in a rectangle with a specific pen.
    /// </summary>
    void DrawRectangle(PointF loc, Size siz, Pen pen);

    /// <summary>
    /// Draw a rectangle in a rectangle with a specific pen.
    /// </summary>
    void DrawRectangle(float x, float y, float width, float height, Pen pen);

    /// <summary>
    /// Fill a rectangle in a rectangle with a specific pen.
    /// </summary>
    void FillRectangle(RectangleF rect, Brush brush);

    /// <summary>
    /// Fill a rectangle in a rectangle with a specific pen.
    /// </summary>
    void FillRectangle(PointF loc, Size siz, Brush brush);

    /// <summary>
    /// Fill a rectangle in a rectangle with a specific pen.
    /// </summary>
    void FillRectangle(float x, float y, float width, float height, Brush brush);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, StringAlignment verAlig, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, StringAlignment verAlig, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Brush brush, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, Brush brush, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, StringAlignment verAlig, Brush brush, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, Brush brush, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, Brush brush, string text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, StringAlignment verAlig, Brush brush, string text);
}