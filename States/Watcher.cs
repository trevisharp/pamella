/* Author:  Leonardo Trevisan Silio
 * Date:    04/04/2023
 */
namespace Pamella.States;

public abstract class Watcher
{
    bool canUpdate = false;
    bool needUpdate = false;
    object lockObj = new object();

    protected abstract void interact();

    public void Interact()
    {
        if (!needUpdate)
            return;
        
        if (!canUpdate)
            return;
        
        lock (lockObj)
        {
            canUpdate = false;
            needUpdate = false;
            
            interact();

            canUpdate = true;
        }
    }

    public void Watch(State state)
        => state.watchers.Add(this);

    internal void OnWatchUpdate()
        => needUpdate = true;
}