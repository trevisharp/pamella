/* Author:  Leonardo Trevisan Silio
 * Date:    04/04/2023
 */
namespace Pamella;

using States;

public abstract class StateView<T> : View
    where T : State
{
    private T state = default(T);
    private T oldState = default(T);

    protected internal virtual void onStart(IGraphics g, State state) { }
    protected internal virtual void onFrame(IGraphics g, State state) { }
    protected internal virtual void onRender(IGraphics g, State state) { }

    protected internal override void OnFrame(IGraphics g)
        => this.onFrame(g, state);

    protected internal override void OnRender(IGraphics g)
        => this.onRender(g, state);

    protected internal override void OnStart(IGraphics g)
        => this.onStart(g, state);
}