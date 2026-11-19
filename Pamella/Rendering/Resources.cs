/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Dynamic;
using System.Linq.Expressions;

namespace Pamella;

/// <summary>
/// Gives information about the construction of a view resource.
/// </summary>
public sealed class ViewContext(View owner)
{
    public View Owner => owner;
}

/// <summary>
/// Represents a sub tree built from a recipe. The cache stores the recipe, not the views.
/// </summary>
public sealed class ViewResource : DynamicObject, ICachedResource
{
    readonly Func<ViewContext, View> recipe;
    readonly ViewNode owner;
    ViewNode root;

    internal ViewResource(Func<ViewContext, View> recipe, ViewNode owner)
    {
        ArgumentNullException.ThrowIfNull(recipe, nameof(recipe));
        this.recipe = recipe;
        this.owner = owner ?? throw new InvalidOperationException("A view resource must be created inside Design.");
    }

    /// <summary>
    /// Get the root view of this resource, building it when needed.
    /// </summary>
    public View Root
    {
        get
        {
            if (root is null)
            {
                root = Build(recipe, owner);
                owner.AddChild(root);
            }
            return (View)(object)root;
        }
    }

    /// <summary>
    /// Get the area of the root view.
    /// </summary>
    public Area Measure()
    {
        _ = Root;
        return root.MeasureArea();
    }

    static ViewNode Build(Func<ViewContext, View> function, ViewNode owner)
    {
        var previous = ViewNode.Owner;
        ViewNode.Owner = owner;
        try { return (ViewNode)(object)function(new ViewContext((View)(object)owner)); }
        finally { ViewNode.Owner = previous; }
    }

    void ICachedResource.ValidateCache(string name)
    {
        if (recipe.Target is not View && !ShaderDefinition.CapturesNothing(recipe))
            throw new InvalidOperationException(
                $"The view {name} is cached, so it cannot capture local variables.");
    }

    object ICachedResource.ForOwner(ViewNode newOwner)
    {
        var function = recipe.Target is View
            ? (Func<ViewContext, View>)Delegate.CreateDelegate(typeof(Func<ViewContext, View>), newOwner, recipe.Method)
            : recipe;
        return new ViewResource(function, newOwner);
    }

    void ICachedResource.WarmInner(ViewNode model)
    {
        try
        {
            Build(recipe, model).Detach();
        }
        catch (Exception ex)
        {
            Scene.Report($"Eager loading of {model.GetType().Name}", ex);
        }
    }

    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
        result = null;
        if (args.Length != 0)
            throw new ArgumentException("A view resource does not receive arguments.");

        if (PaintList.Current is null)
            throw new InvalidOperationException("A view resource can only be called inside Paint.");

        owner.PaintChild(Root);
        return true;
    }
}
