/* Author:  Leonardo Trevisan Silio
 * Date:    03/04/2023
 */
using System.Runtime.InteropServices;

namespace Pamella.Providers.WindowsForms;

/// <summary>
/// A validator and provider for WindowsForms App.
/// </summary>
public class WindowsFormsProvider : ProviderNode
{
    public override bool CanProvide()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public override IApp Provide()
        => new WindowsFormsApp(this);
}