/* Author:  Leonardo Trevisan Silio
 * Date:    03/04/2023
 */
namespace Pamella.Providers;

using Exceptions;

/// <summary>
/// Node of a chain of responsability to provide IGraphics object and
/// Graphical Engine.
/// </summary>
public abstract class ProviderNode
{
    /// <summary>
    /// The Next value in the chain of responsability.
    /// </summary>
    public ProviderNode Next { get; set; }

    /// <summary>
    /// Try provide IApp.
    /// </summary>
    /// <param name="args">Parameters to Iapp creation.</param>
    /// <returns>App object to manage the app screen.</returns>
    public IApp TryProvide()
    {
        if (CanProvide())
            return Provide();
        
        if (Next is null)
            throw new InvalidEnviromentException();
        
        return Next.TryProvide();
    }

    /// <summary>
    /// Return true if this node can provide Igraphics with this parameters. 
    /// </summary>
    /// <param name="args">Parameters to IGraphics creation.</param>
    public abstract bool CanProvide();

    /// <summary>
    /// Return the App provide with this arguments.
    /// </summary>
    /// <param name="args">Parameters to IGraphics creation.</param>
    /// <returns>Graphics object to draw in the app screen.</returns>
    public abstract IApp Provide();   
}