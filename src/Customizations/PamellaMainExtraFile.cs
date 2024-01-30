/* Author:  Leonardo Trevisan Silio
 * Date:    01/01/2023
 */
using Blindness.Abstracts;

namespace Pamella.Customizations;

public class PamellaMainExtraFile : ExtraFile
{
    public PamellaMainExtraFile()
    {
        this.Constant = true;
        this.FileName = "Main.cs";
    }

    public override string Get() =>
    """
    
    """;
}