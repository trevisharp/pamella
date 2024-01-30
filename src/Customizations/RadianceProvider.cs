/* Author:  Leonardo Trevisan Silio
 * Date:    30/01/2024
 */
namespace Pamella.Customizations;

using Providers;

/// <summary>
/// Represents a provider to a app based on radiance technology.
/// </summary>
public class RadianceProvider : ProviderNode
{
    public override bool CanProvide()
        => true;

    public override IApp Provide()
        => new RadianceApp();
}