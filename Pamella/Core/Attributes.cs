/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;

namespace Pamella;

/// <summary>
/// Change this property without invalidating the view.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class UntrackedAttribute : Attribute;

/// <summary>
/// Require this property in the generated constructor function.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute;

/// <summary>
/// Keep this property out of the generated constructor function.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PrivateAttribute : Attribute;

/// <summary>
/// Define the value used when the constructor function does not receive this property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DefaultAttribute(object value) : Attribute
{
    public object Value => value;
}

/// <summary>
/// Define the name of the generated constructor function of a component.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class ConstructorAttribute(string name) : Attribute
{
    public string Name => name;
}
