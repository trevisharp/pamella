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
        PointF[] pts = new PointF[40];

        var center = rect.Center();
                
        return pts;
    }
}