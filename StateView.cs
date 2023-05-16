/* Author:  Leonardo Trevisan Silio
 * Date:    08/04/2023
 */
using Stately;

namespace Pamella;

/// <summary>
/// View associated with a State. The view interact with state on every onFrame execution.
/// The view may render on onRender and start app in onStart.
/// </summary>
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
            => view.OnRender(view.crr);
    }

    public T State => this.state;

    private StateViewWatcher<T> watcher;
    private T state = new T();
    private IGraphics crr = null;

    public StateView()
        => this.watcher = new StateViewWatcher<T>(this);

    protected internal virtual void onStart(IGraphics g, T stt) { }
    protected internal virtual void onFrame(IGraphics g, T stt) { }
    protected internal virtual void onRender(IGraphics g, T stt) { }

    protected internal sealed override void OnFrame(IGraphics g)
    {
        this.crr = g;
        onFrame(g, state);
        watcher.Interact();
    }

    protected internal sealed override void OnRender(IGraphics g)
        => this.onRender(g, this.state);

    protected internal sealed override void OnStart(IGraphics g)
    {
        watcher.Watch(this.state);
        this.crr = g;
        this.onStart(g, this.state);
    }
}