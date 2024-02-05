/* Author:  Leonardo Trevisan Silio
 * Date:    04/02/2024
 */
namespace Pamella;

using Providers;
using Customizations;

/// <summary>
/// Manage your screen.
/// </summary>
public static class Screen
{
    public static PlataformProvider Provider { get; private set; }

    static Screen()
    {
        Provider = new PlataformProvider();
        Provider.Add(new RadianceProvider());
        Blindness.App.Behaviour = new PamellaApp();
    }

    private static IScreenImplementation app = null;
    private static void init()
    {
        if (app is not null)
            return;
        
        app = Provider.Provide();
    }

    /// <summary>
    /// Define a View as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
    /// <param name="view">The view to be displayed on the screen.</param>
    public static void Open(IView view)
    {
        init();
        app.Open(view);
    }

    /// <summary>
    /// Create a new view and define as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
    public static void Open<T>()
        where T : IView
    {
        init();
        Blindness.App.StartWith<T>();
    }

    /// <summary>
    /// Add a view in view stack.
    /// </summary>
    public static void Push(IView view)
    {
        init();
        app.Push(view);
    }

    /// <summary>
    /// Go back to previous view in view stack.
    /// </summary>
    public static void Pop()
    {
        init();
        app.Pop();
    }

    /// <summary>
    /// Clear view stack.
    /// </summary>
    public static void Clear()
    {
        init();
        app.Clear();
    }

    /// <summary>
    /// Close the app.
    /// </summary>
    public static void Close()
    {
        init();
        app.Close();
    }
}