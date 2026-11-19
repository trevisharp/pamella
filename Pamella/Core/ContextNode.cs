/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Pamella;

/// <summary>
/// Represents a logical component, which survives the replacement of its view.
/// </summary>
sealed class ContextNode(ContextNode parent, string slot)
{
    readonly Dictionary<string, ContextNode> children = [];

    public ContextNode Parent => parent;
    public string Slot => slot;
    public IEnumerable<ContextNode> Children => children.Values;
    public ViewNode Instance { get; set; }
    public bool StateStarted { get; set; }
    public Task StateTask { get; set; }
    public CancellationTokenSource StateSource { get; private set; } = new();

    /// <summary>
    /// Get or create the context of a child.
    /// </summary>
    public ContextNode GetChild(string slot)
    {
        if (!children.TryGetValue(slot, out var child))
            children[slot] = child = new ContextNode(this, slot);
        return child;
    }

    /// <summary>
    /// Cancel the running State and allow it to run again.
    /// </summary>
    public void RestartState()
    {
        StateSource.Cancel();
        StateSource = new CancellationTokenSource();
        StateStarted = false;
        StateTask = null;
    }

    /// <summary>
    /// Finish this context, removing it from its parent.
    /// </summary>
    public void End()
    {
        StateSource.Cancel();
        Instance = null;
        parent?.children.Remove(slot);
    }
}
