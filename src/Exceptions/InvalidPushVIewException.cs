/* Author:  Leonardo Trevisan Silio
 * Date:    05/02/2024
 */
using System;

namespace Pamella.Exceptions;

/// <summary>
/// Represents a error that occurs when a non tracked view is pushed.
/// </summary>
public class InvalidPushViewException : Exception
{
    public override string Message 
        => "The view pushed/opened was not caught as a dependency.";
}