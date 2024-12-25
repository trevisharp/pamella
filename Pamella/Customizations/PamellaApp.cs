/* Author:  Leonardo Trevisan Silio
 * Date:    05/02/2023
 */
using System;

using Blindness;
using Blindness.States;
using Blindness.Abstracts;
using Blindness.Concurrency;
using Blindness.Concurrency.Elements;

namespace Pamella.Customizations;

public class PamellaApp(IScreenImplementation implementation) : AppBehaviour
{
    public override void Run<T>(bool debug)
    {
        try
        {
            var model = new DefaultModel();
            DependencySystem.Reset(model);

            var memory = new DefaultMemory();
            Memory.Reset(memory);

            var implementer = new PamellaImplementer();
            if (debug)
                implementer.Implement();
            
            var screen = new ScreenAsyncElement(implementation);
            
            var loopApp = new LoopNodeAppElement<T> {
                Model = model
            };

            var chain = new ReloadLoopElement {
                Model = model,
                First = new HotReload(),
                Second = loopApp
            };

            model.Run(screen);
            model.Run(debug ? chain : loopApp);
            
            model.OnError += (el, er) =>
            {
                Verbose.Error($"On {el} AsyncElement:");
                showError(er);
            };
            model.Start();
        }
        catch (Exception ex)
        {
            showError(ex);
        }
    }
}