/* Author:  Leonardo Trevisan Silio
 * Date:    17/05/2023
 */
using System;

namespace Pamella.Exceptions;

/// <summary>
/// Represents a error of runtime/enviroment for use this framework.
/// </summary>
public class InvalidEnviromentException : Exception
{
    public override string Message 
        => "Invalid or inexistent provider for this runtime";
}