/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq.Expressions;

using Radiance;
using Radiance.Bufferings;

namespace Pamella;

/// <summary>
/// The functions used inside components.
/// </summary>
public static class Utils
{
    static PaintList Painting => PaintList.Current
        ?? throw new InvalidOperationException("This function can only be used inside Paint.");

    static ViewEvents Behaviour => ViewNode.Behaving?.Events
        ?? throw new InvalidOperationException("Events can only be registered inside Behaviour.");

    /// <summary>
    /// Create a shader. The position of the view is applied after its transformations.
    /// </summary>
    public static dynamic shader(Action function)
        => Shader(function);

    /// <summary>
    /// Create a shader. The position of the view is applied after its transformations.
    /// </summary>
    public static dynamic shader<T1>(Action<T1> function)
        => Shader(function);

    /// <summary>
    /// Create a shader. The position of the view is applied after its transformations.
    /// </summary>
    public static dynamic shader<T1, T2>(Action<T1, T2> function)
        => Shader(function);

    /// <summary>
    /// Create a shader. The position of the view is applied after its transformations.
    /// </summary>
    public static dynamic shader<T1, T2, T3>(Action<T1, T2, T3> function)
        => Shader(function);

    /// <summary>
    /// Create a shader. The position of the view is applied after its transformations.
    /// </summary>
    public static dynamic shader<T1, T2, T3, T4>(Action<T1, T2, T3, T4> function)
        => Shader(function);

    /// <summary>
    /// Create a shader. The position of the view is applied after its transformations.
    /// </summary>
    public static dynamic shader<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> function)
        => Shader(function);

    /// <summary>
    /// Create a shader. The position of the view is applied after its transformations.
    /// </summary>
    public static dynamic shader<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> function)
        => Shader(function);

    static ShaderApplication Shader(Delegate function)
    {
        Integration.Install();
        return new ShaderApplication(ShaderDefinition.For(function));
    }

    /// <summary>
    /// Create a sub tree of views owned by the current view.
    /// </summary>
    public static dynamic view(Func<ViewContext, View> recipe)
        => new ViewResource(recipe, ViewNode.Owner);

    /// <summary>
    /// Bind a constructor argument to a expression.
    /// </summary>
    public static Bindable<T> bind<T>(Expression<Func<T>> source)
        => new(source);

    /// <summary>
    /// Make the current view the owner of a resource, released when the view is released.
    /// </summary>
    public static T Draw<T>(T resource)
    {
        if (resource is IDisposable disposable)
            ViewNode.Owner?.AddCleanup(disposable.Dispose);
        return resource;
    }

    /// <summary>
    /// Get a rectangle with a corner on the origin of the view.
    /// </summary>
    public static Polygon Rect(float width, float height)
        => Geometry.Rect(width, height);

    /// <summary>
    /// Restrict what is painted after this call by the current Paint.
    /// </summary>
    public static void Clip(float x, float y, float width, float height)
        => Painting.AddClip(new Area(x, y, width, height));

    /// <summary>
    /// Restrict what is painted after this call by the current Paint.
    /// </summary>
    public static void Clip(Area area)
        => Painting.AddClip(area);

    /// <summary>
    /// Move the children painted after this call.
    /// </summary>
    public static void Offset(float x, float y)
        => Painting.AddOffset(x, y);

    /// <summary>
    /// Get the area of a view or of a view resource.
    /// </summary>
    public static Area Measure(object target) => target switch
    {
        ViewResource resource => resource.Measure(),
        View view => ((ViewNode)(object)view).MeasureArea(),
        _ => throw new ArgumentException("Only views can be measured.", nameof(target))
    };

    public static event Action<Input, Modifier> OnKeyDown
    {
        add => Behaviour.KeyDown += value;
        remove => Behaviour.KeyDown -= value;
    }

    public static event Action<Input, Modifier> OnKeyUp
    {
        add => Behaviour.KeyUp += value;
        remove => Behaviour.KeyUp -= value;
    }

    public static event Action<(float x, float y)> OnMouseMove
    {
        add => Behaviour.MouseMove += value;
        remove => Behaviour.MouseMove -= value;
    }

    public static event Action<MouseButton> OnMouseDown
    {
        add => Behaviour.MouseDown += value;
        remove => Behaviour.MouseDown -= value;
    }

    public static event Action<MouseButton> OnMouseUp
    {
        add => Behaviour.MouseUp += value;
        remove => Behaviour.MouseUp -= value;
    }

    public static event Action<float> OnMouseWheel
    {
        add => Behaviour.MouseWheel += value;
        remove => Behaviour.MouseWheel -= value;
    }

    public static event Action OnMouseEnter
    {
        add => Behaviour.MouseEnter += value;
        remove => Behaviour.MouseEnter -= value;
    }

    public static event Action OnMouseLeave
    {
        add => Behaviour.MouseLeave += value;
        remove => Behaviour.MouseLeave -= value;
    }

    public static event Action<float, float> OnResize
    {
        add => Behaviour.Resize += value;
        remove => Behaviour.Resize -= value;
    }

    public static event Action<string> OnTextInput
    {
        add => Behaviour.TextInput += value;
        remove => Behaviour.TextInput -= value;
    }

    public static event Action<bool> OnFocusChange
    {
        add => Behaviour.FocusChange += value;
        remove => Behaviour.FocusChange -= value;
    }

    public static event Action<string[]> OnFileDrop
    {
        add => Behaviour.FileDrop += value;
        remove => Behaviour.FileDrop -= value;
    }
}
