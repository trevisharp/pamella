/* Author:  Leonardo Trevisan Silio
 * Date:    13/06/2023
 */
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Pamella.Games;

public class Sprite<T> : View
{
    public Rectangle Rect { get; set; }
    public Animation<T> Animation { get; private set; } = new();

    public Sprite()
        => this.AlwaysInvalidateMode();

    protected internal override void OnRender(IGraphics g)
    {
        Animation.Draw(g, Rect);
    }
}