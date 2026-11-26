/* Author:  Leonardo Trevisan Silio
 * Date:    26/11/2026
 */
using System;
using System.Linq;
using System.Dynamic;
using System.Reflection;

using Radiance.Bufferings;
using Radiance.Primitives;

namespace Pamella;

using static Pamella.Utils;

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader();

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader<in T1>(T1 a1);

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader<in T1, in T2>(T1 a1, T2 a2);

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader<in T1, in T2, in T3>(T1 a1, T2 a2, T3 a3);

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader<in T1, in T2, in T3, in T4>(T1 a1, T2 a2, T3 a3, T4 a4);

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader<in T1, in T2, in T3, in T4, in T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader<in T1, in T2, in T3, in T4, in T5, in T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6);

/// <summary>
/// Represents a shader whose remaining arguments are known.
/// </summary>
public delegate void Shader<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7);

/// <summary>
/// Represents a canvas whose arguments are known.
/// </summary>
public delegate void Canvas();

/// <summary>
/// Represents a canvas whose arguments are known.
/// </summary>
public delegate void Canvas<in T1>(T1 a1);

/// <summary>
/// Represents a canvas whose arguments are known.
/// </summary>
public delegate void Canvas<in T1, in T2>(T1 a1, T2 a2);

/// <summary>
/// Represents a canvas whose arguments are known.
/// </summary>
public delegate void Canvas<in T1, in T2, in T3>(T1 a1, T2 a2, T3 a3);

/// <summary>
/// Represents a canvas whose arguments are known.
/// </summary>
public delegate void Canvas<in T1, in T2, in T3, in T4>(T1 a1, T2 a2, T3 a3, T4 a4);

/// <summary>
/// Represents a canvas whose arguments are known.
/// </summary>
public delegate void Canvas<in T1, in T2, in T3, in T4, in T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);

/// <summary>
/// Represents a canvas whose arguments are known.
/// </summary>
public delegate void Canvas<in T1, in T2, in T3, in T4, in T5, in T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6);

/// <summary>
/// Represents a dynamic drawing function that can become a typed Shader or Canvas.
/// </summary>
abstract class DrawingResource : DynamicObject
{
    protected abstract string Family { get; }

    protected abstract string Check(Type[] arguments);

    protected abstract void Run(object[] args);

    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        result = null;
        var type = binder.Type;
        if (!typeof(Delegate).IsAssignableFrom(type) || type.Namespace != "Pamella" || type.Name.Split('`')[0] != Family)
            return base.TryConvert(binder, out result);

        var arguments = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;
        var error = Check(arguments);
        if (error is not null)
            throw new InvalidCastException($"The {Family.ToLower()} {error} and cannot become {Name(type)}.");

        var method = typeof(DrawingResource)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(m => m.Name == nameof(Call) && m.GetGenericArguments().Length == arguments.Length);
        if (arguments.Length > 0)
            method = method.MakeGenericMethod(arguments);

        result = Delegate.CreateDelegate(type, this, method);
        return true;
    }

    internal static string Name(Type type)
    {
        if (type.IsGenericType)
            return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(Name))}>";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Single => "float",
            TypeCode.Double => "double",
            TypeCode.Int32 => "int",
            TypeCode.Boolean => "bool",
            TypeCode.String => "string",
            _ => type.Name
        };
    }

    void Call() => Run([]);
    void Call<T1>(T1 a1) => Run([ a1 ]);
    void Call<T1, T2>(T1 a1, T2 a2) => Run([ a1, a2 ]);
    void Call<T1, T2, T3>(T1 a1, T2 a2, T3 a3) => Run([ a1, a2, a3 ]);
    void Call<T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4) => Run([ a1, a2, a3, a4 ]);
    void Call<T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5) => Run([ a1, a2, a3, a4, a5 ]);
    void Call<T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6) => Run([ a1, a2, a3, a4, a5, a6 ]);
    void Call<T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7) => Run([ a1, a2, a3, a4, a5, a6, a7 ]);
}

/// <summary>
/// The shaders ready to use.
/// </summary>
public static class Shaders
{
    /// <summary>
    /// Get a shader that fills a polygon with a color.
    /// </summary>
    public static Shader<Polygon, Vec4> Fill => field ??= shader((vec4 fillColor) =>
    {
        color = fillColor;
        fill();
    });
}
