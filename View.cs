/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Pamella;

/// <summary>
/// Represents a visualization for the app
/// </summary>
public abstract class View
{
    private View parent = null;
    private List<View> children = new List<View>();
    private bool needRender = true;
    
    public void Draw(Bitmap bmp, Graphics g)
    {
        if (!needRender)
            return;
        
        needRender = false;
        OnRender(bmp, g);
    }

    public void Invalidate()
    {
        needRender = true;

        if (parent is null)
            return;
        parent.needRender = true;
    }

    protected internal abstract void OnRender(Bitmap bmp, Graphics g);
}