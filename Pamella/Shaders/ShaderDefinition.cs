/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.Collections.Generic;

using Radiance;
using Radiance.Bufferings;
using Radiance.Contexts;
using Radiance.Primitives;

namespace Pamella;

/// <summary>
/// Represents a resource that can be stored by [EagerLoading].
/// </summary>
interface ICachedResource
{
    void ValidateCache(string name);
    object ForOwner(ViewNode owner);
    void WarmInner(ViewNode model);
}

/// <summary>
/// Represents the shaders with the same generated code. They share programs and batches.
/// </summary>
sealed class CanonicalShader(Delegate wrapped)
{
    static readonly Dictionary<string, CanonicalShader> known = [];

    public Render Render { get; } = new(wrapped);

    public static CanonicalShader Get(string code, Delegate wrapped)
    {
        if (!known.TryGetValue(code, out var shader))
            known[code] = shader = new CanonicalShader(wrapped);
        return shader;
    }

    public static int Count => known.Count;

    public static void Clear() => known.Clear();
}

/// <summary>
/// Represents a shader function and the layout of its public arguments.
/// </summary>
sealed class ShaderDefinition
{
    static readonly Dictionary<MethodInfo, ShaderDefinition> definitions = [];

    /// <summary>
    /// Get or set the strategy used to add the spatial parameters.
    /// </summary>
    public static IShaderWrapperFactory Wrappers { get; set; } = new GenericShaderWrapperFactory();

    readonly Delegate original;
    readonly Delegate wrapped;
    Render direct;
    CanonicalShader canonical;

    /// <summary>
    /// For each public component, gets if it receives a texture.
    /// </summary>
    public bool[] TextureSlots { get; }
    public int FloatCount { get; }
    public Render Direct => direct ??= new Render(original);

    ShaderDefinition(Delegate function)
    {
        original = function;
        wrapped = Wrappers.Wrap(function);

        List<bool> slots = [];
        foreach (var parameter in function.Method.GetParameters())
        {
            var type = parameter.ParameterType;
            var size = type == typeof(vec2) ? 2
                : type == typeof(vec3) ? 3
                : type == typeof(vec4) ? 4
                : 1;
            for (int i = 0; i < size; i++)
                slots.Add(type == typeof(img));
        }

        TextureSlots = [ ..slots ];
        FloatCount = slots.Count(isTexture => !isTexture);
    }

    /// <summary>
    /// Get the definition of a function that captures nothing.
    /// </summary>
    public static ShaderDefinition For(Delegate function)
    {
        ArgumentNullException.ThrowIfNull(function, nameof(function));
        if (!CapturesNothing(function))
            throw new InvalidOperationException(
                $"The shader {function.Method.Name} captures a variable. Send the values as arguments.");

        if (!definitions.TryGetValue(function.Method, out var definition))
            definitions[function.Method] = definition = new ShaderDefinition(function);
        return definition;
    }

    public static void Clear() => definitions.Clear();

    /// <summary>
    /// Get if a delegate does not hold data of its creation.
    /// </summary>
    public static bool CapturesNothing(Delegate function)
        => function.Target is null || function.Target.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Length == 0;

    /// <summary>
    /// Find the shared shader of this definition. Needs to run on the render phase.
    /// </summary>
    public CanonicalShader Resolve(Polygon polygon, object[] textures)
    {
        if (canonical is not null)
            return canonical;

        var args = new object[TextureSlots.Length + 4];
        args[0] = polygon;
        int textureIndex = 0;
        for (int i = 0; i < TextureSlots.Length; i++)
            args[i + 1] = TextureSlots[i] ? textures[textureIndex++] : Buffers.Create(0f);
        for (int i = 0; i < 3; i++)
            args[TextureSlots.Length + 1 + i] = Buffers.Create(0f);

        ShaderScope.Dry = true;
        ShaderScope.Captured = null;
        try
        {
            new Render(wrapped).TryInvoke(null, args, out _);
            return canonical = CanonicalShader.Get(ShaderScope.Captured ?? "", wrapped);
        }
        finally
        {
            ShaderScope.Dry = false;
            ShaderScope.Captured = null;
        }
    }
}

/// <summary>
/// Represents a shader with some of its arguments already defined.
/// </summary>
sealed class ShaderApplication : DynamicObject, ICachedResource
{
    static readonly object empty = Radiance.Utils.skip;

    readonly ShaderDefinition definition;
    readonly object[] slots;

    public ShaderApplication(ShaderDefinition definition)
    {
        this.definition = definition;
        slots = new object[definition.TextureSlots.Length + 1];
        Array.Fill(slots, empty);
    }

    ShaderApplication(ShaderDefinition definition, object[] slots)
    {
        this.definition = definition;
        this.slots = slots;
    }

    public void ValidateCache(string name) { }
    public object ForOwner(ViewNode owner) => this;
    public void WarmInner(ViewNode model) { }

    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
        result = null;
        if (RenderContext.GetContext() is not null)
        {
            definition.Direct.TryInvoke(binder, args, out result);
            return true;
        }

        var filled = Fill(args);
        var complete = Array.IndexOf(filled, empty) < 0;
        if (!complete || PaintList.Current is null)
        {
            result = new ShaderApplication(definition, filled);
            return true;
        }

        var floats = new float[definition.FloatCount];
        var textures = new object[definition.TextureSlots.Length - definition.FloatCount];
        int floatIndex = 0, textureIndex = 0;
        for (int i = 0; i < definition.TextureSlots.Length; i++)
        {
            if (definition.TextureSlots[i])
                textures[textureIndex++] = filled[i + 1];
            else floats[floatIndex++] = (float)filled[i + 1];
        }

        PaintList.Current.AddDraw(definition, (Polygon)filled[0], floats, textures);
        return true;
    }

    object[] Fill(object[] args)
    {
        var values = Split(args);
        var result = (object[])slots.Clone();
        int slot = 0;
        foreach (var value in values)
        {
            while (slot < result.Length && result[slot] != empty)
                slot++;

            if (slot >= result.Length)
                throw new ArgumentException("The shader received more arguments than it has.");

            result[slot++] = value;
        }

        return result;
    }

    static List<object> Split(object[] args)
    {
        List<object> values = [];
        foreach (var arg in args)
        {
            switch (arg)
            {
                case Vec2 v: values.Add(v.X); values.Add(v.Y); break;
                case Vec3 v: values.Add(v.X); values.Add(v.Y); values.Add(v.Z); break;
                case Vec4 v: values.Add(v.X); values.Add(v.Y); values.Add(v.Z); values.Add(v.W); break;
                case float v: values.Add(v); break;
                case int v: values.Add((float)v); break;
                case double v: values.Add((float)v); break;
                case float[] many: foreach (var v in many) values.Add(v); break;
                case string text: values.Add(Geometry.Text(text, Polygon.DefaultTextSize, null)); break;
                case null: throw new ArgumentNullException(nameof(args), "A shader received a null argument.");
                default: values.Add(arg); break;
            }
        }

        return values;
    }
}
