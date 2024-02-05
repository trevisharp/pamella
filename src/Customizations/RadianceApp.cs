/* Author:  Leonardo Trevisan Silio
 * Date:    01/02/2024
 */
using System.Collections.Generic;
using Blindness.States;
using Radiance;

namespace Pamella.Customizations;

/// <summary>
/// Represents a manager for a App based on Radiance technology.
/// </summary>
public class RadianceApp : IScreenImplementation
{
    int current = -1;
    Stack<int> views = new();

    public void Clear()
    {
        current = null;
        views.Clear();
    }

    public void Close()
        => Window.Close();
    
    public void Open(IView view)
    {
        Clear();
        Push(view);
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
        initRadiance();
    }

    private void initRadiance()
    {
        if (Window.IsOpen)
            return;

        Window.OnMouseMove += p
            => current?.OnMouseMove(p.x, p.y);
        
        Window.OnMouseDown += b
            => current?.OnMouseDown(b);
        
        Window.OnMouseUp += b
            => current?.OnMouseUp(b);

        Window.OnRender += () 
            => current?.Draw();

        Window.Open();
    }
}