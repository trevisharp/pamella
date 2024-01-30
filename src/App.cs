/* Author:  Leonardo Trevisan Silio
 * Date:    08/04/2023
 */
namespace Pamella;

using Providers;

/// <summary>
/// Manage your app.
/// </summary>
public static class App
{
    public static PlataformProvider Provider { get; set; }

    static App()
    {
        Provider = new PlataformProvider();
    }

    private static IApp app = null;
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
    /// <typeparam name="T">The type of new view.</typeparam>
    public static void Open<T>()
        where T : IView, new()
        => Open(new T());

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