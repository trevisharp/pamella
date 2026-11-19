/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System.Reflection;
using System.Linq.Expressions;

using Blindness.Bind;
using Blindness.Bind.Analyzers;
using Blindness.Bind.Behaviors;
using Blindness.Bind.ChainLinks;
using Blindness.Core;
using Blindness.Core.Binds;

namespace Pamella;

/// <summary>
/// Handle bindings over the generated properties of node interfaces.
/// </summary>
sealed class ViewPropertyLink : BindChainLink
{
    /// <summary>
    /// Get if a member is a generated property of a node interface.
    /// </summary>
    public static bool IsViewProperty(MemberInfo member)
        => member is PropertyInfo { DeclaringType.IsInterface: true } property
            && typeof(INode).IsAssignableFrom(property.DeclaringType)
            && property.GetMethod is { IsAbstract: true };

    protected override BindingResult TryHandle(BindingArgs args)
    {
        if (args.Body.RemoveTypeCast() is not MemberExpression { Member: PropertyInfo property } member)
            return BindingResult.Unsuccesfull;

        if (!IsViewProperty(property))
            return BindingResult.Unsuccesfull;

        var owner = Expression.Lambda(member.Expression).Compile().DynamicInvoke();
        var binding = Binding.Get(owner);
        var box = binding.Dictionary.GetBox(property.Name, property.PropertyType);
        return BindingResult.Successful(box, binding);
    }
}

/// <summary>
/// Continue a binding on another chain.
/// </summary>
sealed class ChainAdapter(BindChain chain) : BindChainLink
{
    protected override BindingResult TryHandle(BindingArgs args)
        => chain.Handle(args);
}

/// <summary>
/// Build the left side chain used by Pamella.
/// </summary>
sealed class ViewLeftAnalyzer : IBindAnalyzer
{
    public BindChain BuildChain()
        => BindChain.New()
            .Add(new ViewPropertyLink())
            .Add(new ChainAdapter(new DeepLeftBindAnalyzer().BuildChain()));
}

/// <summary>
/// Build the right side chain used by Pamella.
/// </summary>
sealed class ViewRightAnalyzer : IBindAnalyzer
{
    public BindChain BuildChain()
        => BindChain.New()
            .Add(new ViewPropertyLink())
            .Add(new ChainAdapter(new DefaultRightBindAnalyzer().BuildChain()));
}

/// <summary>
/// Install the binding behaviour of Pamella.
/// </summary>
static class ViewBindings
{
    public static void Install()
        => Binding.SetBehaviour(
            new ViewLeftAnalyzer(),
            new ViewRightAnalyzer(),
            new DefaultBindBehavior()
        );
}
