/* Author:  Leonardo Trevisan Silio
 * Date:    13/06/2023
 */
using System.Drawing;
using System.Collections.Generic;

namespace Pamella.Games;

/// <summary>
/// A controller system for animations.
/// </summary>
public class Animation<T>
{
    private Dictionary<T, SpriteController> states = new();

    public T State { get; set; }

    public void AddSprite(T key, SpriteController controller)
        => this.states.Add(key, controller);

    public void Draw(IGraphics g, RectangleF rect)
    {
        if (!this.states.ContainsKey(this.State))
            return;

        var controller = this.states[this.State];

        if (controller is null)
            return;
        
        controller.Draw(g, rect);
    }
}