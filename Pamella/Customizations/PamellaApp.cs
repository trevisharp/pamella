/* Author:  Leonardo Trevisan Silio
 * Date:    05/02/2023
 */
using System;

using Blindness;
using Blindness.Core;
using Blindness.Injection;
using Blindness.Concurrency;

namespace Pamella.Customizations;

public class PamellaApp(IScreenImplementation implementation) : DefaultAppBehaviour
{
    
}