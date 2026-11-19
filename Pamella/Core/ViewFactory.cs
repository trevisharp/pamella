/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

using Blindness.Bind;
using Blindness.Bind.Boxes;
using Blindness.Core;
using Blindness.Core.Injections;
using Blindness.Injection;

namespace Pamella;

/// <summary>
/// Stores the services available to the Deps functions.
/// </summary>
public sealed class AppServices
{
    readonly Dictionary<Type, Func<object>> services = [];

    /// <summary>
    /// Add a service created once, when first used.
    /// </summary>
    public AppServices Add<T>() where T : class, new()
    {
        var instance = new Lazy<T>(() => new T());
        services[typeof(T)] = () => instance.Value;
        return this;
    }

    /// <summary>
    /// Add a service instance.
    /// </summary>
    public AppServices Add<T>(T instance) where T : class
    {
        services[typeof(T)] = () => instance;
        return this;
    }

    internal bool TryGet(Type type, out object service)
    {
        var found = services.TryGetValue(type, out var factory);
        service = found ? factory() : null;
        return found;
    }
}

/// <summary>
/// Create nodes using the services of the application.
/// </summary>
sealed class ViewDepFunction : DepFunction
{
    public override object Call(Type type, DependencySystem depSys, InjectionArgs args)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes)
            ?? throw new MissingMethodException($"The type {type} needs a empty constructor.");

        try
        {
            var obj = constructor.Invoke([]);
            var deps = type.GetMethod("Deps");
            if (deps is not null)
            {
                var values = deps.GetParameters()
                    .Select(parameter => ViewFactory.Services.TryGet(parameter.ParameterType, out var service)
                        ? service
                        : depSys.Get(parameter.ParameterType, args))
                    .ToArray();
                deps.Invoke(obj, values);
            }

            if (obj is Node node)
                node.Load();

            return obj;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}

/// <summary>
/// Create and configure views. Used by the generated constructor functions.
/// </summary>
public static class ViewFactory
{
    static readonly ViewDepFunction function = new();
    static readonly ConcreteFilter filter = new();

    /// <summary>
    /// Get the services available to the Deps functions.
    /// </summary>
    public static AppServices Services { get; internal set; } = new();

    /// <summary>
    /// Create a view from its interface.
    /// </summary>
    public static T Create<T>() where T : View
        => (T)(object)Create(typeof(T));

    internal static ViewNode Create(Type type)
    {
        Integration.Install();
        type = DependencySystem.Shared.GetCompatibilityType(type) ?? type;
        return (ViewNode)DependencySystem.Shared.Get(type, function, [ filter ]);
    }

    /// <summary>
    /// Register a property defined by a constructor argument.
    /// </summary>
    public static void MarkArgument(View view, string property) { }

    /// <summary>
    /// Make a view the parent of another.
    /// </summary>
    public static void Adopt(View parent, View child)
        => ((ViewNode)(object)parent).AddChild((ViewNode)(object)child);

    /// <summary>
    /// Make a view the parent of many others.
    /// </summary>
    public static void Adopt(View parent, IEnumerable<View> children)
    {
        foreach (var child in children)
            Adopt(parent, child);
    }

    /// <summary>
    /// Bind a property of a view to a expression.
    /// </summary>
    public static void Bind<T>(View view, Type owner, string property, Expression<Func<T>> source)
    {
        var info = owner.GetProperty(property);
        var body = source.Body.RemoveTypeCast();
        if (body is MemberExpression { Member: PropertyInfo member } && ViewPropertyLink.IsViewProperty(member))
        {
            var left = Expression.Property(Expression.Constant(view, owner), info);
            var same = typeof(ViewFactory)
                .GetMethod(nameof(Same), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(typeof(T));
            view.Bind(Expression.Lambda<Func<bool>>(Expression.Equal(left, source.Body, false, same)));
            return;
        }

        var computed = new ComputedBox<T>(source.Compile());
        var box = Binding.Get(view).Dictionary.GetBox(property, typeof(T));
        Box.SetInner(box, computed);
        ((ViewNode)(object)view).AddCleanup(computed.Dispose);
    }

    static bool Same<T>(T left, T right)
        => EqualityComparer<T>.Default.Equals(left, right);
}

/// <summary>
/// Stores the generated views and loads their shared resources.
/// </summary>
public static class ViewRegistry
{
    sealed class Entry(Type view, Type concrete, bool eager)
    {
        public Type View => view;
        public Type Concrete => concrete;
        public bool Eager => eager;
        public bool Started { get; set; }
    }

    static readonly Dictionary<string, Entry> byName = [];
    static readonly Dictionary<Type, Entry> byConcrete = [];

    [ThreadStatic] static bool creatingModel;
    [ThreadStatic] internal static List<ICachedResource> Captured;

    /// <summary>
    /// Register a generated view.
    /// </summary>
    public static void Register(Type view, Type concrete, bool eager)
    {
        var entry = new Entry(view, concrete, eager);
        byName[view.FullName] = entry;
        byConcrete[concrete] = entry;
    }

    internal static Type FindView(string fullName)
        => byName.TryGetValue(fullName, out var entry) ? entry.View : null;

    internal static Type FindByConcreteName(string fullName)
        => byName.Values.FirstOrDefault(entry => entry.Concrete.FullName == fullName)?.View;

    internal static bool TakeModelFlag()
    {
        var value = creatingModel;
        creatingModel = false;
        return value;
    }

    /// <summary>
    /// Load the shared resources of all the registered views.
    /// </summary>
    internal static void WarmAll()
    {
        foreach (var entry in byName.Values.ToArray())
            Warm(entry);
    }

    internal static void EnsureWarm(Type concrete)
    {
        if (byConcrete.TryGetValue(concrete, out var entry))
            Warm(entry);
    }

    static void Warm(Entry entry)
    {
        if (!entry.Eager || entry.Started)
            return;

        entry.Started = true;
        var outerCaptured = Captured;
        Captured = [];
        ViewNode model = null;
        try
        {
            creatingModel = true;
            model = ViewFactory.Create(entry.Concrete);
            model.CaptureEager();

            var captured = Captured;
            Captured = null;
            foreach (var resource in captured)
                resource.WarmInner(model);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"The Design function of {entry.View.Name} failed during eager loading. "
                + "Design also runs without parent, arguments and State.", ex);
        }
        finally
        {
            creatingModel = false;
            Captured = outerCaptured;
            model?.Detach();
        }
    }
}
