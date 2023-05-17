/* Author:  Leonardo Trevisan Silio
 * Date:    16/05/2023
 */
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Pamella.Views;

/// <summary>
/// View to draw a clicable button.
/// </summary>
public class Button : StateView<ButtonState>
{
    public string Text
    {
        get => State.Text;
        set => State.Text |= value;
    }

    public Color BackColor
    {
        get => State.BackColor;
        set => State.BackColor |= value;
    }

    public Color SelectedColor
    {
        get => State.SelectedColor;
        set => State.SelectedColor |= value;
    }

    public Color PressedColor
    {
        get => State.PressedColor;
        set => State.PressedColor |= value;
    }

    public Color BorderColor
    {
        get => State.BorderColor;
        set => State.BorderColor |= value;
    }

    public Color SelectedBorderColor
    {
        get => State.SelectedBorderColor;
        set => State.SelectedBorderColor |= value;
    }

    public Color PressedBorderColor
    {
        get => State.PressedBorderColor;
        set => State.PressedBorderColor |= value;
    }

    public float BorderWidth
    {
        get => State.BorderWidth;
        set => State.BorderWidth |= value;
    }

    public RectangleF Rect
    {
        get => State.Rect;
        set
        {
            State.Rect |= value;
            this.poly = null;
        }
    }

    public float CornerRadius
    {
        get => State.CornerRadius;
        set
        {
            State.CornerRadius |= value;
            this.poly = null;
        }
    }
    
    public Action<Button, PointF> OnMouseDown;
    public Action<Button, PointF> OnMouseUp;

    private PointF[] poly = null;
    private Brush backBrush = null;
    private Brush selectedBrush = null;
    private Brush pressedBrush = null;
    private Pen backPen = null;
    private Pen selectedPen = null;
    private Pen pressedPen = null;

    private bool isDown = false;

    private void init(ButtonState stt)
    {
        this.backBrush = new SolidBrush(stt.BackColor);
        this.selectedBrush = new SolidBrush(stt.SelectedColor);
        this.pressedBrush = new SolidBrush(stt.PressedColor);

        this.backPen = new Pen(stt.BorderColor, BorderWidth);
        this.selectedPen = new Pen(stt.SelectedBorderColor, BorderWidth);
        this.pressedPen = new Pen(stt.PressedBorderColor, BorderWidth);

        if (this.poly is not null)
            return;

        RectangleF rect = stt.Rect;

        float widsize = stt.CornerRadius;
        float limit = rect.Width / 2;
        widsize = widsize > limit ? limit : widsize; 

        float heisize = stt.CornerRadius;
        limit = rect.Height / 2;
        heisize = heisize > limit ? limit : heisize; 

        List<PointF> pts = new List<PointF>();

        float xstart = rect.X;
        float xend = rect.X + rect.Width;

        float ystart = rect.Y;
        float yend = rect.Y + rect.Height;

        float xradstart = rect.X + widsize;
        float xradend = rect.X + rect.Width - widsize;

        float yradstart = rect.Y + heisize;
        float yradend = rect.Y + rect.Height - heisize;

        float reference = rect.X + rect.Width / 2;

        pts.Add(new PointF(xradend, ystart));
        connect(pts, xradend, ystart, xend, yradstart, reference);
        pts.Add(new PointF(xend, yradstart));

        pts.Add(new PointF(xend, yradend));
        connect(pts, xend, yradend, xradend, yend, reference);
        pts.Add(new PointF(xradend, yend));

        pts.Add(new PointF(xradstart, yend));
        connect(pts, xradstart, yend, xstart, yradend, reference);
        pts.Add(new PointF(xstart, yradend));

        pts.Add(new PointF(xstart, yradstart));
        connect(pts, xstart, yradstart, xradstart, ystart, reference);
        pts.Add(new PointF(xradstart, ystart));

        this.poly = pts.ToArray();
    }

    /// <summary>
    /// Add points in a list to connect two points using a ellipse.
    /// </summary>
    /// <param name="sx">start point x-cordinate</param>
    /// <param name="sy">start point y-cordinate</param>
    /// <param name="ex">end point x-cordinate</param>
    /// <param name="ey">end point y-cordinate</param>
    private void connect(List<PointF> list,
        float sx, float sy,
        float ex, float ey,
        float xreference
    )
    {
        float a = sx - ex;
        if (a < 0)
            a = -a;
        
        float b = sy - ey;
        if (b < 0)
            b = -b;

        bool startIsRef = MathF.Abs(sx - xreference) < MathF.Abs(ex - xreference);
        float ox = startIsRef ? sx : ex;
        float oy = startIsRef ? ey : sy;
        
        float A = a * a;
        float C = b / a;
        
        if (sx < ex)
        {
            for (float x = sx - ox; x < ex - ox; x++)
            {
                float y = C * MathF.Sqrt(A - x * x);
                list.Add(new PointF(x + ox, oy - y));
            }
        }
        else
        {
            for (float x = sx - ox; x >= ex - ox; x--)
            {
                float y = C * MathF.Sqrt(A - x * x);
                list.Add(new PointF(x + ox, y + oy));
            }
        }
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
            
            if (g.IsDown && !this.isDown && OnMouseDown != null)
                OnMouseDown(this, g.Cursor);

            if (!g.IsDown && this.isDown && OnMouseUp != null)
                OnMouseUp(this, g.Cursor);

            this.isDown = g.IsDown;
        }
        else stt.Selected |= false;
    }

    protected internal override void onRender(IGraphics g, ButtonState stt)
    {
        init(stt);

        if (stt.Pressed)
        {
            g.FillPolygon(this.poly, this.pressedBrush);
            g.DrawPolygon(this.poly, this.pressedPen);
        }
        else if (stt.Selected)
        {
            g.FillPolygon(this.poly, this.selectedBrush);
            g.DrawPolygon(this.poly, this.selectedPen);
        }
        else
        {
            g.FillPolygon(this.poly, this.backBrush);
            g.DrawPolygon(this.poly, this.backPen);
        }

        g.DrawText(
            this.Rect, 
            StringAlignment.Center, 
            StringAlignment.Center, 
            this.Text
        );
    }
}