/* Author:  Leonardo Trevisan Silio
 * Date:    01/01/2023
 */
using Blindness.Abstracts;
using Blindness.Abstracts.Implementations;

namespace Pamella.Customizations;

public class PamellaImplementer : Implementer
{
    public PamellaImplementer()
    {
        this.BaseConcreteType = typeof(View);
        this.Implementations.Add(new ConstructorImplementation());
        this.Implementations.Add(new ConcreteImplementation());
        this.Implementations.Add(new DefaultUsingsImplementation());
        this.Implementations.Add(new DepsImplementation());
        this.Implementations.Add(new OnLoadImplementation());
        this.Implementations.Add(new OnRunImplementation());
        this.Implementations.Add(new DefaultPropertyImplementation());

        this.ExtraFiles.Add(new PamellaGlobalImportsExtraFile());
    }
}