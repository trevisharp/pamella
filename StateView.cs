/* Author:  Leonardo Trevisan Silio
 * Date:    05/04/2023
 */
namespace Pamella;

using States;

public abstract class StateView<T> : Watcher
    where T : State, new()
{
    private T state = new T();
    private IGraphics crr = null;

    protected override void interact()
        => this.onFrame(this.state, this.crr);

    protected internal virtual void onStart(IGraphics g, State state) { }
    protected internal virtual void onFrame(IGraphics g, State state) { }
    protected internal virtual void onRender(IGraphics g, State state) { }

    protected internal override void OnFrame(IGraphics g)
    {
        this.crr = g;
        this.Interact();
    }

    protected internal override void OnRender(IGraphics g)
        => this.onRender(g, this.state);

    protected internal override void OnStart(IGraphics g)
    {
        Watch(this.state);
        this.crr = g;
        this.onStart(g, this.state);
    }
}