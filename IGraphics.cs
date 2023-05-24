/* Author:  Leonardo Trevisan Silio
 * Date:    23/05/2023
 */
using System;
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
    /// Add event on key down.
    /// </summary>
    void SubscribeKeyDownEvent(Action<Input> ev);

    /// <summary>
    /// Add event on key up.
    /// </summary>
    void SubscribeKeyUpEvent(Action<Input> ev);

    /// <summary>
    /// Remove event on key down.
    /// </summary>
    void UnsubscribeKeyDownEvent(Action<Input> ev);

    /// <summary>
    /// Remove event on key up.
    /// </summary>
    void UnsubscribeKeyUpEvent(Action<Input> ev);

    /// <summary>
    /// Clear a screen with a specific color.
    /// </summary>
    void Clear(Color color);

    /// <summary>
    /// Draw a rectangle in a rectangle with a specific pen.
    /// </summary>
    void DrawRectangle(RectangleF rect, Pen pen)
        => DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, pen);

    /// <summary>
    /// Draw a rectangle in a rectangle with a specific pen.
    /// </summary>
    void DrawRectangle(PointF loc, Size siz, Pen pen)
        => DrawRectangle(loc.X, loc.Y, siz.Width, siz.Height, pen);

    /// <summary>
    /// Draw a rectangle in a rectangle with a specific pen.
    /// </summary>
    void DrawRectangle(float x, float y, float width, float height, Pen pen);

    /// <summary>
    /// Fill a rectangle in a rectangle with a specific pen.
    /// </summary>
    void FillRectangle(RectangleF rect, Brush brush)
        => FillRectangle(rect.X, rect.Y, rect.Width, rect.Height, brush);

    /// <summary>
    /// Fill a rectangle in a rectangle with a specific pen.
    /// </summary>
    void FillRectangle(PointF loc, Size siz, Brush brush)
        => FillRectangle(loc.X, loc.Y, siz.Width, siz.Height, brush);

    /// <summary>
    /// Fill a rectangle in a rectangle with a specific pen.
    /// </summary>
    void FillRectangle(float x, float y, float width, float height, Brush brush);

    /// <summary>
    /// Fill a polygon with a specific pen.
    /// </summary>
    void DrawPolygon(PointF[] poly, Pen pen);

    /// <summary>
    /// Fill a polygon with a specific pen.
    /// </summary>
    void FillPolygon(PointF[] poly, Brush brush);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, string text)
        => DrawText(rect, SystemFonts.MenuFont, StringAlignment.Center, StringAlignment.Center, Brushes.Black, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, string text)
        => DrawText(rect, SystemFonts.MenuFont, horAlig, StringAlignment.Center, Brushes.Black, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, StringAlignment verAlig, string text)
        => DrawText(rect, SystemFonts.MenuFont, horAlig, verAlig, Brushes.Black, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, string text)
        => DrawText(rect, font, StringAlignment.Center, StringAlignment.Center, Brushes.Black, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, string text)
        => DrawText(rect, font, StringAlignment.Center, StringAlignment.Center, Brushes.Black, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, StringAlignment verAlig, string text)
        => DrawText(rect, font, horAlig, verAlig, Brushes.Black, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Brush brush, string text)
        => DrawText(rect, SystemFonts.MenuFont, StringAlignment.Center, StringAlignment.Center, brush, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, Brush brush, string text)
        => DrawText(rect, SystemFonts.MenuFont, horAlig, StringAlignment.Center, brush, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, StringAlignment horAlig, StringAlignment verAlig, Brush brush, string text)
        => DrawText(rect, SystemFonts.MenuFont, horAlig, verAlig, brush, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, Brush brush, string text)
        => DrawText(rect, font, StringAlignment.Center, StringAlignment.Center, brush, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, Brush brush, string text)
        => DrawText(rect, font, horAlig, StringAlignment.Center, brush, text);

    /// <summary>
    /// Draw a Text in a Rectangle with a font and color.
    /// </summary>
    void DrawText(RectangleF rect, Font font, StringAlignment horAlig, StringAlignment verAlig, Brush brush, string text);

    /// <summary>
    /// Draw a subrect of a image in the a rectangle in the scree.
    /// </summary>
    void DrawImage(RectangleF rect, Image img, RectangleF sourceRect);

    /// <summary>
    /// Draw a image in the a rectangle in the scree.
    /// </summary>
    void DrawImage(RectangleF rect, Image img)
        => DrawImage(rect, img, new RectangleF(0, 0, img.Width, img.Height));
}