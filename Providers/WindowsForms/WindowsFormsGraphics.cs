/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
using System;

namespace Pamella.Providers.WindowsForms;

/// <summary>
/// A WindowsForms IGraphics Implementatino
/// </summary>
public class WindowsFormsGraphics : IGraphics
{
    public WindowsFormsGraphics(WindowsFormsProviderArguments args)
    {
        
    }

    public int Width => throw new NotImplementedException();

    public int Height => throw new NotImplementedException();
}