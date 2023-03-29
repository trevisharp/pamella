/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
namespace Pamella;

/// <summary>
/// Represents a visualization for the app.
/// </summary>
public abstract class View
{
    private bool needRender = true;
    
    /// <summary>
    /// Draw this view using a render flag conditional.
    /// </summary>
    /// <param name="g">Graphics implementation parameter.</param>
    public void Draw(IGraphics g)
    {
        if (!needRender)
            return;
        
        needRender = false;
        OnRender(g);
    }

    /// <summary>
    /// Set render flag to true indicating a need to render on Draw method.
    /// </summary>
    public void Invalidate()
        => needRender = true;

    /// <summary>
    /// Draw this view unconditionally.
    /// </summary>
    /// <param name="g">Graphics implementation parameter.</param>
    protected internal abstract void OnRender(IGraphics g);
}