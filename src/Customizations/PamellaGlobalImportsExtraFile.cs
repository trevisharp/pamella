/* Author:  Leonardo Trevisan Silio
 * Date:    01/01/2023
 */
using Blindness.Abstracts;

namespace Pamella.Customizations;

public class PamellaGlobalImportsExtraFile : ExtraFile
{
    public PamellaGlobalImportsExtraFile()
    {
        this.Constant = true;
        this.FileName = "Usings.cs";
    }

    public override string Get() =>
    """
    global using static Radiance.RadianceUtils;
    """;
}

