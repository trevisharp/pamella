/* Author:  Leonardo Trevisan Silio
 * Date:    17/05/2023
 */
using System;

namespace Pamella.Exceptions;

/// <summary>
/// Represents a error of subview bad usage.
/// </summary>
public class InvalidSubViewException : Exception
{
    public override string Message 
        => "This view already contains a parent.";
}