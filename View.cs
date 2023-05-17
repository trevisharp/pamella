/* Author:  Leonardo Trevisan Silio
 * Date:    16/05/2023
 */
namespace Pamella;

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
        if (!initializated)
        {
            OnStart(g);
            initializated = true;
        }

        OnFrame(g);

        if (!needRender)
            return;
        
        needRender = alwaysInvalidate;
        OnRender(g);
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
}