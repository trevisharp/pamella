/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System.Threading;
using System.Collections.Generic;

using Blindness.Core;
using Blindness.Factory;

namespace Pamella;

/// <summary>
/// Represents a unit of composition, position and drawing.
/// </summary>
[Ignore]
[ConcreteBase(typeof(ViewNode))]
public interface View : INode
{
    private ViewNode Self => (ViewNode)(object)this;

    /// <summary>
    /// Get or set the horizontal position relative to the parent.
    /// </summary>
    float LocalX { get; set; }

    /// <summary>
    /// Get or set the vertical position relative to the parent.
    /// </summary>
    float LocalY { get; set; }

    /// <summary>
    /// Get the horizontal position on the window.
    /// </summary>
    float X => Self.GlobalX;

    /// <summary>
    /// Get the vertical position on the window.
    /// </summary>
    float Y => Self.GlobalY;

    /// <summary>
    /// Get the view that contains this view.
    /// </summary>
    View Parent => Self.ParentNode as View;

    /// <summary>
    /// Get or set the depth group of this view and of its children.
    /// </summary>
    Layer Layer
    {
        get => Self.ViewLayer;
        set => Self.ViewLayer = value;
    }

    /// <summary>
    /// Get the token cancelled when this component really dies.
    /// </summary>
    CancellationToken StateToken => Self.StateToken;

    /// <summary>
    /// Get the token cancelled when this instance is replaced.
    /// </summary>
    CancellationToken LoadToken => Self.LoadToken;

    /// <summary>
    /// Request a new Paint of this view.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Run the State function of this component again.
    /// </summary>
    void ReloadState() => Self.ReloadState();

    /// <summary>
    /// Paint a child view at this point of the paint order.
    /// </summary>
    void Paint(View child) => Self.PaintChild(child);

    /// <summary>
    /// Paint many child views at this point of the paint order.
    /// </summary>
    void Paint(IEnumerable<View> children)
    {
        foreach (var child in children)
            Self.PaintChild(child);
    }
}
