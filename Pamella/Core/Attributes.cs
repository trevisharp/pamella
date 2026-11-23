/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;

namespace Pamella;

/// <summary>
/// Invalidate the view when the value of this property changes.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class InvalidateOnChangeAttribute : Attribute;

/// <summary>
/// Load this resource on application start and share it between all the instances.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EagerLoadingAttribute : Attribute;

/// <summary>
/// Require this property in the generated constructor function.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute;

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
