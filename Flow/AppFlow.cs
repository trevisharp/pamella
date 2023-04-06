/* Author:  Leonardo Trevisan Silio
 * Date:    04/04/2023
 */
using FlowPattern;

namespace Pamella.Flow;

using States;

public class AppFlow<T, V> : Flow<T>
        where T : State, new()
        where V : StateView<T>, new()
{
    private V view;
    private AppFlow()
        => this.view = new V();

    public override void Start()
    {
        
    }

    public static AppFlow<T, V> Init()
        => new AppFlow<T, V>();
}