/* Author:  Leonardo Trevisan Silio
 * Date:    03/04/2023
 */
using System;

namespace Pamella.Providers;

/// <summary>
/// A object to provide the best IGraphics/Graphical engine implementation for this
/// specific enviroment/runtime.
/// </summary>
public class PlataformProvider
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
    public IApp Provide()
    {
        if (root is null)
            throw new InvalidOperationException("Not provider added yet.");
        
        return this.root.TryProvide();
    }

    public void Clear()
        => this.root = null;
}