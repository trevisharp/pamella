/* Author:  Leonardo Trevisan Silio
 * Date:    31/01/2023
 */
using System.Collections.Generic;

using Blindness;

namespace Pamella;

using Exceptions;

/// <summary>
/// Base type for all views in app.
/// </summary>
public abstract partial class View : Node
{
    public IEnumerable<View> SubViews => this.subviews;
    public Ownership Ownership { get; set; }
    
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

        frameIfNeeded();

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

    private void renderIfNeeded()
    {
        if (!needRender)
        {
            renderSubViews();
            return;
        }
        needRender = false;

        OnRender();
        renderSubViews();
        
        if (invalidationMode.NeedInvalidate(this))
            Invalidate();
    }

    private void frameIfNeeded()
    {
        OnFrame();
        frameSubViews();
    }

    private void startIfNeeded()
    {
        if (initializated)
        {
            startSubViews();
            return;
        }
        
        initializated = true;
        OnStart();
        startSubViews();
    }

    // Subview system
    private View parent = null;

    /// <summary>
    /// Add a subview for this view.
    /// </summary>
    public void AddSubView(View view)
    {
        if (view is null)
            return;

        if (view.parent is not null)
            throw new InvalidSubViewException();
    
        if (subviews is null)
            subviews = new List<View>();
        
        view.parent = this;
        subviews.Add(view);
    }

    /// <summary>
    /// Add subviews for this view.
    /// </summary>
    public void AddSubView(params View[] views)
    {
        if (views is null)
            return;

        foreach (var view in views)
            AddSubView(view);
    }

    /// <summary>
    /// Get or Set all subviews from this view.
    /// </summary>
    /// <value>A Collection of views</value>
    public ICollection<View> Content
    {
        get => this.subviews.ToArray();
        set => this.subviews = new List<View>(value);
    }

    private void renderSubViews()
    {
        if (subviews is null)
            return;
        
        foreach (var view in this.subviews)
            view.renderIfNeeded();
    }

    private void startSubViews()
    {
        if (subviews is null)
            return;
        
        foreach (var view in this.subviews)
            view.startIfNeeded();
    }

    private void frameSubViews()
    {
        if (subviews is null)
            return;
        
        foreach (var view in this.subviews)
            view.frameIfNeeded();
    }
}