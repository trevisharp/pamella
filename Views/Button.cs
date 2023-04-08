/* Author:  Leonardo Trevisan Silio
 * Date:    08/04/2023
 */
using static System.MathF;
using System.Drawing;
using System.Collections.Generic;

namespace Pamella.Views;

/// <summary>
/// View to draw a clicable button.
/// </summary>
public class Button : StateView<ButtonState>
{
    private PointF[] poly = null;
    private Brush backBrush = null;
    private Brush selectedBrush = null;
    private Brush pressedBrush = null;

    private void init(ButtonState stt)
    {
        this.backBrush = new SolidBrush(stt.BackColor);
        this.selectedBrush = new SolidBrush(stt.SelectedColor);
        this.pressedBrush = new SolidBrush(stt.PressedColor);

        float radius = stt.CornerRadius;
        radius = radius > 1f ? 1f : radius;
        radius = radius < 0f ? 0f : radius;
        radius /= 2;

        RectangleF rect = stt.Rect;
        List<PointF> pts = new List<PointF>();

        float xstart = rect.X;
        float xend = rect.X + rect.Width;

        float xradstart = rect.X + radius * rect.Width;
        float xradend = rect.X + (1f - radius) * rect.Width;

        float ystart = rect.Y;
        float yend = rect.Y + rect.Height;

        float yradstart = rect.Y + radius * rect.Height;
        float yradend = rect.Y + (1f - radius) * rect.Height;

        pts.Add(new PointF(xradstart, ystart));
        pts.Add(new PointF(xradend, ystart));

        pts.Add(new PointF(xend, yradstart));
        pts.Add(new PointF(xend, yradend));

        pts.Add(new PointF(xradend, yend));
        pts.Add(new PointF(xradstart, yend));

        pts.Add(new PointF(xstart, yradend));
        pts.Add(new PointF(xstart, yradstart));

        this.poly = pts.ToArray();
    }

    protected internal override void onStart(IGraphics g, ButtonState stt)
    {
        
    }

    protected internal override void onFrame(IGraphics g, ButtonState stt)
    {
        RectangleF rect = stt.Rect;
        if (rect.Contains(g.Cursor))
        {
            stt.Selected |= true;
            stt.Pressed |= g.IsDown;
        }
        else stt.Selected |= false;
    }

    protected internal override void onRender(IGraphics g, ButtonState stt)
    {
        init(stt);

        if (stt.Pressed)
            g.FillPolygon(this.poly, this.pressedBrush);
        else if (stt.Selected)
            g.FillPolygon(this.poly, this.selectedBrush);
        else
            g.FillPolygon(this.poly, this.backBrush);
    }
}