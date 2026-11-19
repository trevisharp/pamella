/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Reflection;

using Radiance;

namespace Pamella;

/// <summary>
/// Builds the function that adds the spatial parameters to a shader.
/// </summary>
public interface IShaderWrapperFactory
{
    Delegate Wrap(Delegate original);
}

/// <summary>
/// Builds wrappers from generic functions written once for each parameter count.
/// </summary>
public sealed class GenericShaderWrapperFactory : IShaderWrapperFactory
{
    static readonly MethodInfo[] builders = typeof(GenericShaderWrapperFactory)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
        .Where(method => method.Name == nameof(Build))
        .OrderBy(method => method.GetGenericArguments().Length)
        .ToArray();

    public Delegate Wrap(Delegate original)
    {
        var types = original.GetType().IsGenericType
            ? original.GetType().GetGenericArguments()
            : Type.EmptyTypes;

        if (types.Length == 0)
            return Build((Action)original);

        return (Delegate)builders[types.Length]
            .MakeGenericMethod(types)
            .Invoke(null, [ original ]);
    }

    static void Run(val pmX, val pmY, val pmZ, Action body)
    {
        ShaderScope.Offset = Radiance.Utils.vec(pmX, pmY, pmZ);
        try { body(); }
        finally { ShaderScope.Offset = null; }
    }

    static Action<val, val, val> Build(Action o)
        => (pmX, pmY, pmZ) => Run(pmX, pmY, pmZ, o);

    static Action<T1, val, val, val> Build<T1>(Action<T1> o)
        => (a1, pmX, pmY, pmZ) => Run(pmX, pmY, pmZ, () => o(a1));

    static Action<T1, T2, val, val, val> Build<T1, T2>(Action<T1, T2> o)
        => (a1, a2, pmX, pmY, pmZ) => Run(pmX, pmY, pmZ, () => o(a1, a2));

    static Action<T1, T2, T3, val, val, val> Build<T1, T2, T3>(Action<T1, T2, T3> o)
        => (a1, a2, a3, pmX, pmY, pmZ) => Run(pmX, pmY, pmZ, () => o(a1, a2, a3));

    static Action<T1, T2, T3, T4, val, val, val> Build<T1, T2, T3, T4>(Action<T1, T2, T3, T4> o)
        => (a1, a2, a3, a4, pmX, pmY, pmZ) => Run(pmX, pmY, pmZ, () => o(a1, a2, a3, a4));

    static Action<T1, T2, T3, T4, T5, val, val, val> Build<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> o)
        => (a1, a2, a3, a4, a5, pmX, pmY, pmZ) => Run(pmX, pmY, pmZ, () => o(a1, a2, a3, a4, a5));

    static Action<T1, T2, T3, T4, T5, T6, val, val, val> Build<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> o)
        => (a1, a2, a3, a4, a5, a6, pmX, pmY, pmZ) => Run(pmX, pmY, pmZ, () => o(a1, a2, a3, a4, a5, a6));
}
