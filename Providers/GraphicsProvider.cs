/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
using System;

namespace Pamella.Providers;

/// <summary>
/// A object to provide the best IGraphics implementation for this
/// specific enviroment/runtime.
/// </summary>
public class GraphicsProvider
{
    private ProviderNode root = null;
    
    /// <summary>
    /// Add a possibile provider to provide IGraphics.
    /// </summary>
    /// <param name="provider">A implementation of provider node.</param>
    public void Add(ProviderNode provider)
    {
        if (this.root is null)
        {
            this.root = provider;
            return;
        }

        var crr = this.root;
        while (crr.Next != null)
            crr = crr.Next;
        
        crr.Next = provider;
    }

    /// <summary>
    /// Provide IGraphics from this enviroment/runtime.
    /// </summary>
    /// <param name="args">Args needed to generate IGraphics.</param>
    /// <returns>Graphics object to draw in the app screen.s</returns>
    public IGraphics Provide(ProviderArgument args)
    {
        if (args is null)
            throw new ArgumentNullException("args");
        
        if (root is null)
            throw new InvalidOperationException("Not provider added yet.");
        
        return this.root.TryProvide(args);
    }
}