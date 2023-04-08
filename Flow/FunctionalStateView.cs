/* Author:  Leonardo Trevisan Silio
 * Date:    07/04/2023
 */
using System;
using Stately;

namespace Pamella.Flow;

/// <summary>
/// A State view that store delegates for his operations.
/// </summary>
public class FunctionalStateView<T> : StateView<T>
    where T : State, new()
{
    public FunctionalStateView<T> OnRender(Action<IGraphics, T> render)
    {
        this.render = render;
        return this;
    }
    
    public FunctionalStateView<T> OnFrame(Action<IGraphics, T> frame)
    {
        this.frame = frame;
        return this;
    }
    
    public FunctionalStateView<T> OnStart(Action<IGraphics, T> start)
    {
        this.start = start;
        return this;
    }

    private Action<IGraphics, T> frame;
    private Action<IGraphics, T> render;
    private Action<IGraphics, T> start;

    protected internal override void onFrame(IGraphics g, T state)
    {
        if (frame is null)
            return;
        frame(g, state);
    }

    protected internal override void onRender(IGraphics g, T state)
    {
        if (render is null)
            return;
        render(g, state);
    }

    protected internal override void onStart(IGraphics g, T state)
    {
        if (start is null)
            return;
        start(g, state);
    }
}