/* Author:  Leonardo Trevisan Silio
 * Date:    24/05/2023
 */
using System.Collections.Generic;

namespace Pamella;

using Exceptions;

/// <summary>
/// Represents a visualization for the app.
/// </summary>
public abstract partial class View
{
    private bool alwaysInvalidate = false;
    private bool initializated = false;
    private bool needRender = true;
    
    /// <summary>
    /// Draw this view using a render flag conditional.
    /// </summary>
    /// <param name="g">Graphics implementation parameter.</param>
    public void Draw(IGraphics g)
    {
        startIfNeeded(g);

        frameIfNeeded(g);

        renderIfNeeded(g);
    }

    /// <summary>
    /// Set render flag to true indicating a need to render on Draw method.
    /// </summary>
    public void Invalidate()
    {
        needRender = true;

        if (this.subviews is null)
            return;

        foreach (var view in this.subviews)
            view.Invalidate();
    }

    /// <summary>
    /// Set the view component to permanently invalidated.
    /// </summary>
    public void AlwaysInvalidateMode()
        => this.alwaysInvalidate = true;
    
    /// <summary>
    /// Set the view component as a component that need invalidate to render.
    /// </summary>
    public void NeedInvalidateMode()
        => this.alwaysInvalidate = false;

    /// <summary>
    /// Draw this view unconditionally.
    /// </summary>
    /// <param name="g">Graphics implementation parameter.</param>
    protected internal virtual void OnRender(IGraphics g) { }

    /// <summary>
    /// Run every frame of application.
    /// </summary>
    /// <param name="g"></param>
    protected internal virtual void OnFrame(IGraphics g) { }

    /// <summary>
    /// Run before first render.
    /// </summary>
    /// <param name="g"></param>
    protected internal virtual void OnStart(IGraphics g) { }

    private void renderIfNeeded(IGraphics g)
    {
        if (!needRender)
        {
            renderSubViews(g);
            return;
        }
        needRender = false;

        OnRender(g);
        renderSubViews(g);
        
        if (alwaysInvalidate)
            needRender = true;
    }

    private void frameIfNeeded(IGraphics g)
    {
        OnFrame(g);
        frameSubViews(g);
    }

    private void startIfNeeded(IGraphics g)
    {
        if (initializated)
        {
            startSubViews(g);
            return;
        }
        
        initializated = true;
        OnStart(g);
        startSubViews(g);
    }

    // Subview system
    private View parent = null;
    private protected List<View> subviews = null;

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

    private void renderSubViews(IGraphics g)
    {
        if (subviews is null)
            return;
        
        foreach (var view in this.subviews)
            view.renderIfNeeded(g);
    }

    private void startSubViews(IGraphics g)
    {
        if (subviews is null)
            return;
        
        foreach (var view in this.subviews)
            view.startIfNeeded(g);
    }

    private void frameSubViews(IGraphics g)
    {
        if (subviews is null)
            return;
        
        foreach (var view in this.subviews)
            view.frameIfNeeded(g);
    }
}