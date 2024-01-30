/* Author:  Leonardo Trevisan Silio
 * Date:    30/01/2024
 */
using System;

namespace Pamella.Providers;

/// <summary>
/// A object to provide the best IApp engine implementation for this
/// specific enviroment/runtime.
/// </summary>
public class PlataformProvider
{
    private ProviderNode root = null;
    private ProviderNode last = null;
    
    /// <summary>
    /// Add a possibile provider to provide IGraphics.
    /// </summary>
    public PlataformProvider Add(ProviderNode provider)
    {
        if (root is null)
        {
            init(provider);
            return this;
        }
        
        last.Next = provider;
        last = last.Next;
        return this;
    }

    /// <summary>
    /// Provide IGraphics from this enviroment/runtime.
    /// </summary>
    public IApp Provide()
    {
        if (root is null)
            throw new InvalidOperationException("Not provider added yet.");
        
        return this.root.TryProvide();
    }

    /// <summary>
    /// Clear all providers
    /// </summary>
    public void Clear()
        => init(null);
    
    private void init(ProviderNode node)
        => root = last = node;
}