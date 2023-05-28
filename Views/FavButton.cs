/* Author:  Leonardo Trevisan Silio
 * Date:    17/05/2023
 */
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Pamella.Views;

using States;

public class FavButton : StateView<FavButtonState>
{
    public RectangleF Rect
    {
        get => State.Rect;
        set => State.Rect |= value;
    }

    public bool Checked
    {
        get => State.Checked;
        set => State.Checked |= value;
    }

    public event Action<FavButton, PointF> OnToggle;

    private bool isDown = false;

    protected internal override void onRender(IGraphics g, FavButtonState stt)
    {
    }

    protected internal override void onFrame(IGraphics g, FavButtonState stt)
    {
        RectangleF rect = stt.Rect;
        if (rect.Contains(g.Cursor))
        {
            stt.Selected |= true;
            
            if (g.IsDown && !isDown)
                isDown = true;
            else if (!g.IsDown && !isDown)
            {
                State.Checked |= !State.Checked;
                if (OnToggle != null)
                    OnToggle(this, g.Cursor);
            }

            this.isDown = g.IsDown;
        }
        else stt.Selected |= false;
    }

    private PointF[] poly;

    private void init(FavButtonState stt)
    {
        
    }

    private PointF[] genHeart(RectangleF rect, float size)
    {
        const int count = 40;
        const float limit = 2 * MathF.PI;
        float step = limit / count;

        PointF[] pts = new PointF[count];

        float a = 16f;
        float b = 13f;
        float c = 5f;
        float d = 1f;

        var center = rect.Center();

        float t = 0f;
        for (int i = 0; i < count; i++)
        {
            pts[i] = new PointF(x(t) + center.X, y(t) + center.Y);
            t += step;
        }

        return pts;

        float x(float t)
        {
            var sin =  MathF.Sin(t);
            return a * sin * sin * sin;
        }

        float y(float t)
        {
            return d * (b * MathF.Cos(t) - c * MathF.Cos(2 * t) - 3 *  MathF.Cos(3 * t) - MathF.Cos(4 * t));
        }
    }
}