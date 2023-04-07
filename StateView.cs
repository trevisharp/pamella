/* Author:  Leonardo Trevisan Silio
 * Date:    05/04/2023
 */
using Stately;

namespace Pamella;

public abstract class StateView<T> : View
    where T : State, new()
{
    private class StateViewWatcher<R> : Watcher
        where R : State, new()
    {
        StateView<R> view;
        public StateViewWatcher(StateView<R> view)
            => this.view = view;
        
        protected override void interact()
        {
            view.onFrame(view.crr, view.state);
        }
    }

    private StateViewWatcher<T> watcher;
    private T state = new T();
    private IGraphics crr = null;

    public StateView()
        => this.watcher = new StateViewWatcher<T>(this);

    protected internal virtual void onStart(IGraphics g, State state) { }
    protected internal virtual void onFrame(IGraphics g, State state) { }
    protected internal virtual void onRender(IGraphics g, State state) { }

    protected internal override void OnFrame(IGraphics g)
    {
        this.crr = g;
        watcher.Interact();
    }

    protected internal override void OnRender(IGraphics g)
        => this.onRender(g, this.state);

    protected internal override void OnStart(IGraphics g)
    {
        watcher.Watch(this.state);
        this.crr = g;
        this.onStart(g, this.state);
    }
}