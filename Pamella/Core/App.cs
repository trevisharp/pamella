/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Threading;
using System.Collections.Concurrent;

using Radiance;
using Radiance.Primitives;

namespace Pamella;

/// <summary>
/// Represents the configuration of a application.
/// </summary>
public sealed class AppConfig
{
    public string Title { get; set; } = "Pamella";
    public (int width, int height) Size { get; set; } = (800, 600);
    public WindowMode Mode { get; set; } = WindowMode.Windowed;
    public Vec4 Background { get; set; } = new(0, 0, 0, 1);
    public bool HotReload { get; set; } = Blindness.Core.App.Debug;
    public AppServices Services { get; } = new();
}

/// <summary>
/// Runs the continuations of await on the thread of the window.
/// </summary>
sealed class UiContext : SynchronizationContext
{
    static readonly ConcurrentQueue<(SendOrPostCallback Callback, object State)> queue = new();

    public override void Post(SendOrPostCallback d, object state)
    {
        queue.Enqueue((d, state));
        Scene.RequestFrame();
    }

    public static void Drain()
    {
        while (queue.TryDequeue(out var item))
        {
            try { item.Callback(item.State); }
            catch (Exception ex) { Scene.Report("Continuation", ex); }
        }
    }
}

/// <summary>
/// Opens and controls a Pamella application.
/// </summary>
public static class App
{
    /// <summary>
    /// Open the window showing a view. Returns when the window closes.
    /// </summary>
    public static void Run<T>(Action<AppConfig> configure = null) where T : View
    {
        Integration.Install();

        var config = new AppConfig();
        configure?.Invoke(config);
        ViewFactory.Services = config.Services;

        Window.Title = config.Title;
        Window.Size = config.Size;
        Window.Mode = config.Mode;
        Window.RenderMode = RenderMode.OnDemand;

        Scene.Initialize();
        InputRouter.Attach();

        Window.OnLoad += () =>
        {
            Window.ClearColor = config.Background;
            SynchronizationContext.SetSynchronizationContext(new UiContext());
            ViewRegistry.WarmAll();
            Scene.PushRoot(ViewFactory.Create(typeof(T)));
        };
        Window.OnRender += Scene.Frame;
        Window.OnUnload += Scene.Close;

        Window.Open();
    }

    /// <summary>
    /// Show a view over the current one.
    /// </summary>
    public static void Push<T>(Action<T> configure = null) where T : View
    {
        var view = ViewFactory.Create<T>();
        configure?.Invoke(view);
        Scene.PushRoot((ViewNode)(object)view);
    }

    /// <summary>
    /// End the current view and return to the previous one.
    /// </summary>
    public static void Pop() => Scene.PopRoot();

    /// <summary>
    /// Close the window.
    /// </summary>
    public static void Close() => Window.Close();
}
