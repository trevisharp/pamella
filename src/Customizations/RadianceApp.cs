/* Author:  Leonardo Trevisan Silio
 * Date:    30/01/2024
 */
using System.Collections.Generic;

using Radiance;

namespace Pamella.Customizations;

/// <summary>
/// Represents a manager for a App based on Radiance technology.
/// </summary>
public class RadianceApp : IApp
{
    IView current = null;
    Stack<IView> views = new();

    public void Clear()
    {
        current = null;
        views.Clear();
    }

    public void Close()
    {
        Window.Close();
    }

    public void Open(IView view)
    {
        Push(view);

        Window.OnMouseMove += p
            => current?.OnMouseMove(p.x, p.y);
        
        Window.OnMouseDown += b
            => current?.OnMouseDown(b);
        
        Window.OnMouseUp += b
            => current?.OnMouseUp(b);
        
        Window.OnFrame += ()
            => current?.OnFrame();

        Window.OnRender += () 
            => current?.Draw();

        Window.Open();
    }

    public void Pop()
    {
        views.Pop();
        current = views.Peek();
    }

    public void Push(IView view)
    {
        current = view;
        views.Push(view);
    }
}