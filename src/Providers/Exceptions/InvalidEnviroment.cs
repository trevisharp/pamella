/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
using System;

namespace Pamella.Providers.Exceptions;

/// <summary>
/// Represents a error of runtime/enviroment for use this framework.
/// </summary>
public class InvalidEnviroment : Exception
{
    public override string Message 
        => "Invalid or inexistent provider for this runtime";
}