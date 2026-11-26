/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Dynamic;
using System.Linq.Expressions;

namespace Pamella;

/// <summary>
/// Represents a drawing function that receives a Graphics from the paint context.
/// </summary>
sealed class CanvasResource : DrawingResource
{
    readonly Delegate function;
    readonly Type[] publicTypes;
    readonly int graphicsIndex;
    readonly Action<object[]> invoke;

    public CanvasResource(Delegate function)
    {
        ArgumentNullException.ThrowIfNull(function, nameof(function));
        this.function = function;

        var types = function.Method.GetParameters().Select(p => p.ParameterType).ToArray();
        graphicsIndex = Array.IndexOf(types, typeof(Graphics));
        publicTypes = types.Where(type => type != typeof(Graphics)).ToArray();

        var array = Expression.Parameter(typeof(object[]));
        var values = types.Select((type, i) => (Expression)Expression.Convert(
            Expression.ArrayIndex(array, Expression.Constant(i)), type));
        invoke = Expression
            .Lambda<Action<object[]>>(Expression.Invoke(Expression.Constant(function), values), array)
            .Compile();
    }

    protected override string Family => "Canvas";

    protected override string Check(Type[] arguments)
        => arguments.SequenceEqual(publicTypes)
            ? null
            : $"expects ({string.Join(", ", publicTypes.Select(Name))})";

    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
        result = null;
        Run(args);
        return true;
    }

    protected override void Run(object[] args)
    {
        if (PaintList.Current is null)
            throw new InvalidOperationException("A canvas can only be called inside Paint.");

        if (args.Length != publicTypes.Length)
            throw new ArgumentException($"The canvas expects {publicTypes.Length} arguments and received {args.Length}.");

        var offset = graphicsIndex < 0 ? 0 : 1;
        var values = new object[args.Length + offset];
        for (int i = 0, source = 0; i < values.Length; i++)
        {
            if (i == graphicsIndex)
            {
                values[i] = Graphics.Current;
                continue;
            }

            values[i] = Coerce(args[source], publicTypes[source]);
            source++;
        }

        invoke(values);
    }

    static object Coerce(object value, Type type)
        => value is null || type.IsInstanceOfType(value) || value is not IConvertible
            ? value
            : Convert.ChangeType(value, type);
}

/// <summary>
/// Gives information about the construction of a view resource.
/// </summary>
public sealed class ViewContext(View owner)
{
    public View Owner => owner;
}

/// <summary>
/// Represents a sub tree built from a recipe.
/// </summary>
public sealed class ViewResource : DynamicObject
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
