/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
using System;

namespace Pamella.Providers.WindowsForms;

/// <summary>
/// A validator and provider for WindowsForms Graphics
/// </summary>
public class WindowsFormsProvider : ProviderNode
{
    public override bool CanProvide(ProviderArgument args) =>
        args is not null &&
        args is WindowsFormsProviderArguments winargs &&
        this.canProvide(winargs);

    private bool canProvide(WindowsFormsProviderArguments winargs)
    {
        if (winargs.Bitmap is null)
            return false;
        
        if (winargs.Graphics is null)
            return false;
        
        if (winargs.PictureBox is null)
            return false;
        
        return true;
    }

    public override IGraphics Provide(ProviderArgument args)
    {
        if (args is null)
            throw new ArgumentNullException("args");
        
        return new WindowsFormsGraphics(args as WindowsFormsProviderArguments);
    }
}