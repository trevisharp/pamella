/* Author:  Leonardo Trevisan Silio
 * Date:    17/05/2023
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
        startSubViews(g);

        frameIfNeeded(g);
        frameSubViews(g);

        renderIfNeeded(g);
        renderSubViews(g);
    }

    /// <summary>
    /// Set render flag to true indicating a need to render on Draw method.
    /// </summary>
    public void Invalidate()
        => needRender = true;

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
    protected internal abstract void OnRender(IGraphics g);

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
            return;
        
        OnRender(g);
        needRender = alwaysInvalidate;
    }

    private void frameIfNeeded(IGraphics g)
    {
        OnFrame(g);
    }

    private void startIfNeeded(IGraphics g)
    {
        if (initializated)
            return;
        
        initializated = true;
        OnStart(g);
    }

    // Subview system
    private View parent = null;
    private List<View> subviews = null;

    public void AddSubView(View view)
    {
        if (view.parent is not null)
            throw new InvalidSubViewException();
    
        if (subviews is null)
            subviews = new List<View>();
        
        view.parent = this;
        subviews.Add(view);
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