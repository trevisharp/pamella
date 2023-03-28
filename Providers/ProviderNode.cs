/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
namespace Pamella.Providers;

using Exceptions;

/// <summary>
/// Node of a chain of responsability to provide IGraphics object
/// </summary>
public abstract class ProviderNode
{
    /// <summary>
    /// The Next value in the chain of responsability.
    /// </summary>
    public ProviderNode Next { get; set; }

    /// <summary>
    /// Try provide IGraphics.
    /// </summary>
    /// <param name="args">Parameters to IGraphics creation.</param>
    /// <returns>Graphics object to draw in the app screen.</returns>
    public IGraphics TryProvide(ProviderArgument args)
    {
        if (CanProvide(args))
            return Provide(args);
        
        if (Next is null)
            throw new InvalidEnviroment();
        
        return Next.TryProvide(args);
    }

    /// <summary>
    /// Return true if this node can provide Igraphics with this parameters. 
    /// </summary>
    /// <param name="args">Parameters to IGraphics creation.</param>
    public abstract bool CanProvide(ProviderArgument args);

    /// <summary>
    /// Return the IGraphics provide with this arguments.
    /// </summary>
    /// <param name="args">Parameters to IGraphics creation.</param>
    /// <returns>Graphics object to draw in the app screen.</returns>
    public abstract IGraphics Provide(ProviderArgument args);   
}