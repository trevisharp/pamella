/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;

using Radiance;
using Radiance.Windows;
using Radiance.Contexts;
using Radiance.Bufferings;
using Radiance.Primitives;
using Radiance.CodeGeneration;
using Radiance.Implementations;

namespace Pamella;

/// <summary>
/// Stores the spatial data of the shader being analyzed.
/// </summary>
static class ShaderScope
{
    [ThreadStatic] public static vec3 Offset;
    [ThreadStatic] public static bool Dry;
    [ThreadStatic] public static string Captured;
}

/// <summary>
/// Apply the position of the view as the last transformation of a shader.
/// </summary>
sealed class ScopedGenerator(ICodeGenerator inner) : ICodeGenerator
{
    public ShaderPair GenerateShaders(
        vec3 vertObj,
        vec4 fragObj,
        IShaderConfiguration ctx,
        GeneratorOptions options = null,
        boolean visible = null)
        => inner.GenerateShaders(
            ShaderScope.Offset is null ? vertObj : vertObj + ShaderScope.Offset,
            fragObj, ctx, options, visible);
}

/// <summary>
/// Add the behaviour of Pamella to the Radiance implementation in use.
/// </summary>
sealed class ScopedFactory(IImplementationFactory inner) : IImplementationFactory
{
    public ShaderContext NewContext()
        => ShaderScope.Dry ? new DryContext() : inner.NewContext();

    public BaseWindow NewWindow()
        => inner.NewWindow();

    public ICodeGenerator NewCodeGenerator()
        => new ScopedGenerator(inner.NewCodeGenerator());
}

/// <summary>
/// A shader context that only captures the generated code.
/// </summary>
sealed class DryContext : ShaderContext
{
    public override void SetFloat(string name, float value) { }
    public override void SetVec(string name, float x, float y) { }
    public override void SetVec(string name, float x, float y, float z) { }
    public override void SetVec(string name, float x, float y, float z, float w) { }
    public override void SetTextureData(string name, Texture texture) { }
    public override void Dispose() { }
    public override void CreateProgram(ShaderPair pair, bool verbose = false)
        => ShaderScope.Captured += pair.VertexShader.Code + "\n" + pair.FragmentShader.Code + "\n";
    public override void Draw(PrimitiveType primitiveType, IBufferedData data) { }
    public override void InitArgs(object[] args) { }
    public override void UseArgs(object[] args) { }
    public override void UseProgram() { }
    public override void SetLineWidth(float width) { }
    public override void SetPointSize(float size) { }
}

/// <summary>
/// Install the extensions of Pamella on Radiance and Blindness.
/// </summary>
static class Integration
{
    static bool installed;

    public static void Install()
    {
        if (installed)
            return;

        installed = true;
        ImplementationConfig.Implementation = new ScopedFactory(ImplementationConfig.Implementation);
        ViewBindings.Install();
    }
}
