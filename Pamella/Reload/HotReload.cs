/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Blindness;
using Blindness.Bind;
using Blindness.Bind.Boxes;
using Blindness.Generators;
using Blindness.Injection;
using Blindness.Reload;

using Pamella.Generators;

namespace Pamella;

/// <summary>
/// Compiles the project with the generators of Blindness and Pamella.
/// </summary>
sealed class ViewCompiler : AssemblyCompiler
{
    protected override Assembly GetNewAssembly(string directory, IEnumerable<Assembly> extraRefs)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: PreprocessorSymbols);
        var trees = FindAllCSharpFiles(directory)
            .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file), parseOptions, file));

        var compilation = CSharpCompilation.Create(
            "PamellaHotReload",
            trees,
            GetReferences(extraRefs),
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        CSharpGeneratorDriver
            .Create(new ConcreteGenerator(), new ViewGenerator())
            .WithUpdatedParseOptions(parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generated, out _);

        using var stream = new MemoryStream();
        var result = generated.Emit(stream);
        if (!result.Success)
        {
            foreach (var error in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                Console.Error.WriteLine($"[Pamella] {error}");
            return null;
        }

        stream.Seek(0, SeekOrigin.Begin);
        return new AssemblyLoadContext($"PamellaHotReload-{Guid.NewGuid():N}").LoadFromStream(stream);
    }
}

/// <summary>
/// Replaces the views when the source code changes, keeping the components alive.
/// </summary>
static class HotReload
{
    static Assembly pending;
    static int generation;
    static int compositionBefore = -1;

    public static bool Enabled { get; private set; }
    public static int Generation => generation;

    /// <summary>
    /// Watch the project when the application runs from its folder.
    /// </summary>
    public static void Start()
    {
        var directory = Environment.CurrentDirectory;
        if (Directory.GetFiles(directory, "*.csproj").Length == 0)
            return;

        Enabled = true;
        var watcher = new FileWatcher().AddCSharpFilter();
        var compiler = new ViewCompiler { MainDirectory = directory };
        watcher.Verify();

        var thread = new Thread(() =>
        {
            while (Radiance.Window.IsOpen)
            {
                Thread.Sleep(200);
                if (watcher.Verify())
                    Request(compiler);
            }
        }) { IsBackground = true, Name = "Pamella hot reload" };
        thread.Start();
    }

    static void Request(AssemblyCompiler compiler)
    {
        try
        {
            var assembly = compiler.Get();
            if (assembly is null)
                return;

            Volatile.Write(ref pending, assembly);
            Scene.RequestFrame();
        }
        catch (Exception ex)
        {
            Scene.Report("Hot reload", ex);
        }
    }

    /// <summary>
    /// Replace the views by the ones of the compiled assembly.
    /// </summary>
    public static void ApplyPending()
    {
        var assembly = Interlocked.Exchange(ref pending, null);
        if (assembly is not null)
            Apply(assembly);
    }

    /// <summary>
    /// Replace the views by the ones of a assembly.
    /// </summary>
    internal static void Apply(Assembly assembly)
    {
        var old = Scene.Root;
        if (old is null)
            return;

        DependencySystem.Shared.UpdateAssembly(assembly);
        RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);

        var name = old.GetType().FullName;
        var view = ViewRegistry.FindByConcreteName(name);
        if (view is null)
            return;

        generation++;
        compositionBefore = Scene.CompositionId;
        Scene.ResetShaders();

        old.Detach();
        Scene.ReplaceRoot(ViewFactory.Create(view));
    }

    /// <summary>
    /// End the components that the new code does not show anymore.
    /// </summary>
    public static void FinishPending()
    {
        if (compositionBefore < 0)
            return;

        var before = compositionBefore;
        compositionBefore = -1;
        Finish(Scene.RootContext, before);
    }

    static void Finish(ContextNode context, int before)
    {
        foreach (var child in context.Children.ToArray())
            Finish(child, before);

        var instance = context.Instance;
        if (instance is null || instance.Generation == generation || instance.ComposedAt != before)
            return;

        instance.EndReplaced();
    }
}

/// <summary>
/// Copies values between versions of a type.
/// </summary>
static class Migrator
{
    public static bool TryConvert(object value, Type target, out object result)
    {
        try
        {
            result = Convert(value, target, new Dictionary<object, object>(ReferenceEqualityComparer.Instance));
            return true;
        }
        catch (Exception ex)
        {
            Scene.Report($"Hot reload could not keep a {target.Name}", ex);
            result = null;
            return false;
        }
    }

    static object Convert(object value, Type target, Dictionary<object, object> seen)
    {
        if (value is null)
            return null;

        if (target.IsInstanceOfType(value))
            return value;

        if (seen.TryGetValue(value, out var known))
            return known;

        if (target.IsEnum)
            return Enum.Parse(target, value.ToString());

        if (target.IsArray && value is IEnumerable sequence)
        {
            var items = sequence.Cast<object>().ToArray();
            var array = Array.CreateInstance(target.GetElementType(), items.Length);
            seen[value] = array;
            for (int i = 0; i < items.Length; i++)
                array.SetValue(Convert(items[i], target.GetElementType(), seen), i);
            return array;
        }

        var instance = Activator.CreateInstance(target);
        seen[value] = instance;

        if (instance is IDictionary dictionary && value is IDictionary source)
        {
            var types = target.GetGenericArguments();
            foreach (DictionaryEntry entry in source)
                dictionary[Convert(entry.Key, types[0], seen)] = Convert(entry.Value, types[1], seen);
            return instance;
        }

        if (instance is IList list && value is IEnumerable values)
        {
            var type = target.GetGenericArguments()[0];
            foreach (var item in values)
                list.Add(Convert(item, type, seen));
            return instance;
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        foreach (var property in target.GetProperties(flags).Where(p => p.CanWrite && p.GetIndexParameters().Length == 0))
        {
            var old = value.GetType().GetProperty(property.Name, flags);
            if (old is not null)
                property.SetValue(instance, Convert(old.GetValue(value), property.PropertyType, seen));
        }

        foreach (var field in target.GetFields(flags))
        {
            var old = value.GetType().GetField(field.Name, flags);
            if (old is not null)
                field.SetValue(instance, Convert(old.GetValue(value), field.FieldType, seen));
        }

        return instance;
    }
}

public abstract partial class ViewNode
{
    readonly Dictionary<string, object> baseline = [];
    readonly HashSet<string> arguments = [];

    internal int Generation { get; } = HotReload.Generation;

    internal void MarkArgument(string property)
    {
        if (HotReload.Enabled)
            arguments.Add(property);
    }

    IEnumerable<PropertyInfo> DataProperties => GetType()
        .GetProperties()
        .Where(p => p.PropertyType != typeof(object) && p.GetCustomAttribute<BindingAttribute>() is not null);

    static bool IsValueBox(object box)
    {
        var type = Box.GetInner(box).GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueBox<>);
    }

    partial void CaptureBaseline()
    {
        if (!HotReload.Enabled)
            return;

        foreach (var property in DataProperties)
            baseline[property.Name] = property.GetValue(this);
    }

    bool ChangedSinceMount(string name, object value)
    {
        if (value is not null && value is not string && !value.GetType().IsValueType)
            return true;

        return !Equals(value, baseline.GetValueOrDefault(name));
    }

    partial void TakeOver(ViewNode old)
    {
        var loading = Context.StateTask is { IsCompleted: false };
        var mine = Binding.Get(this).Dictionary;
        var theirs = Binding.Get(old).Dictionary;

        foreach (var property in DataProperties)
        {
            var source = old.GetType().GetProperty(property.Name);
            if (source is null || source.GetCustomAttribute<BindingAttribute>() is null || arguments.Contains(property.Name))
                continue;

            var target = mine.GetBox(property.Name, property.PropertyType);
            var origin = theirs.GetBox(property.Name, source.PropertyType);
            if (!IsValueBox(target) || !IsValueBox(origin))
                continue;

            var value = Box.Open(origin);
            if (!loading && !old.ChangedSinceMount(property.Name, value))
                continue;

            if (source.PropertyType == property.PropertyType)
                Box.SetInner(target, Box.GetInner(origin));
            else if (Migrator.TryConvert(value, property.PropertyType, out var converted))
                property.SetValue(this, converted);
        }
    }

    /// <summary>
    /// Run the end of a component replaced by code that does not show it anymore.
    /// </summary>
    internal void EndReplaced()
    {
        Scene.Observe(CallUnload(), this, "Unload");
        Context?.End();
    }
}
