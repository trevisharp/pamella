/* Author:  Leonardo Trevisan Silio
 * Date:    05/02/2024
 */
using System;
using System.Collections.Generic;

using Radiance;

namespace Pamella.Customizations;

using Exceptions;

/// <summary>
/// Represents a manager for a App based on Radiance technology.
/// </summary>
public class RadianceApp : IScreenImplementation
{
    int currentPointer = -1;
    IView current => throw new NotImplementedException();
    Stack<int> views = new();

    public void Clear()
    {
        currentPointer = -1;
        views.Clear();
    }

    public void Close()
        => Window.Close();
    
    public void Open()
    {
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

    public void Pop()
    {
        views.Pop();
        currentPointer = views.Peek();
    }

    public void Push(IView view)
    {
        throw new NotImplementedException();
    }
}