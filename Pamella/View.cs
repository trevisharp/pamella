/* Author:  Leonardo Trevisan Silio
 * Date:    31/01/2023
 */
using System.Collections.Generic;
using System.Collections.Immutable;

using Blindness;

namespace Pamella;

using Exceptions;

/// <summary>
/// Base type for all views in app.
/// </summary>
public abstract partial class View : Node
{
    public Ownership Ownership { get; set; }
    public ImmutableArray<View> SubViews
        => this.subviews.ToImmutableArray();
    
    private View parent = null;
    private List<View> subviews = [];
    private AutoInvalidationMode invalidationMode = new AlwaysInvalidate();
    private bool initializated = false;
    private bool needRender = true;
    
    /// <summary>
    /// Draw this view using a render flag conditional.
    /// </summary>
    public void Draw()
    {
        startIfNeeded();
        frame();
        renderIfNeeded();
    }

    /// <summary>
    /// Set render flag to true indicating a need to render on Draw method.
    /// </summary>
    public void Invalidate()
    {
        needRender = true;

        foreach (var view in this.subviews)
            view.Invalidate();
    }

    /// <summary>
    /// Set the view autoinvalidation mode.
    /// </summary>
    public void SetInvalidateMode(AutoInvalidationMode mode)
        => this.invalidationMode = mode;

    /// <summary>
    /// Add a subview for this view.
    /// </summary>
    private void AddSubView(View view)
    {
        if (view is null)
            return;

        if (view.parent is not null)
            throw new InvalidSubViewException();
        
        view.parent = this;
        subviews.Add(view);
    }

    /// <summary>
    /// Add subviews for this view.
    /// </summary>
    public void AddSubView(params View[] views)
    {
        foreach (var view in views)
            AddSubView(view);
    }

    /// <summary>
    /// Draw this view unconditionally.
    /// </summary>
    protected internal virtual void OnRender() { }

    /// <summary>
    /// Run every frame of application.
    /// </summary>
    protected internal virtual void OnFrame() { }

    /// <summary>
    /// Run before first render.
    /// </summary>
    protected internal virtual void OnStart() { }

    private void frame()
    {
        foreach (var view in this.subviews)
            view.frame();
        OnFrame();
    }

    private void renderIfNeeded()
    {
        if (needRender)
        {
            OnRender();
            needRender = false;
        }

        foreach (var view in subviews)
            view.renderIfNeeded();
        
        if (invalidationMode.NeedInvalidate(this))
            Invalidate();
    }

    private void startIfNeeded()
    {
        foreach (var view in subviews)
            view.startIfNeeded();
        
        if (initializated)
            return;
        initializated = true;
        
        OnStart();
    }
    
}