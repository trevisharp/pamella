/* Author:  Leonardo Trevisan Silio
 * Date:    01/01/2023
 */
using Blindness.Factory;

namespace Pamella.Customizations;

public class PamellaImplementer : Implementer
{
    public PamellaImplementer() : base(typeof(View))
    {
        this.ExtraFiles.Add(new PamellaGlobalImportsExtraFile());
    }
}