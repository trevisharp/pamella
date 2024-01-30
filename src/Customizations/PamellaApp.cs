/* Author:  Leonardo Trevisan Silio
 * Date:    30/01/2023
 */
using System;

using Blindness;
using Blindness.States;
using Blindness.Internal;
using Blindness.Abstracts;
using Blindness.Concurrency;
using Blindness.Concurrency.Elements;

namespace Pamella.Customizations;

public class PamellaApp : AppBehaviour
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
            
            var loopApp = new LoopNodeAppElement<T> {
                Model = model
            };
            
            var chain = new ReloadLoopElement {
                Model = model,
                First = new HotReload(),
                Second = loopApp
            };

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