/* Author:  Leonardo Trevisan Silio
 * Date:    25/12/2024
 */
using Blindness.Factory;

namespace Pamella.Customizations;

public class PamellaGlobalImportsExtraFile : ExtraFile
{
    public PamellaGlobalImportsExtraFile()
    {
        Constant = true;
        FileName = "Usings.cs";
    }

    public override string Get() =>
    """
    global using Radiance;
    global using static Radiance.Utils;
    """;
}