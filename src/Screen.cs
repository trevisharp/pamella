/* Author:  Leonardo Trevisan Silio
 * Date:    05/02/2024
 */
namespace Pamella;

using Providers;
using Customizations;

/// <summary>
/// Manage your screen.
/// </summary>
public static class Screen
{
    public static IScreenImplementation Implementation { get; private set; }
    public static PlataformProvider Provider { get; private set; }

    static Screen()
    {
        Provider = new PlataformProvider();
        Provider.Add(new RadianceProvider());
    }

    private static void init()
    {
        if (Implementation is not null)
            return;
        
        Implementation = Provider.Provide();
    }

    /// <summary>
    /// Define a View as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
    /// <param name="view">The view to be displayed on the screen.</param>
    public static void Open<T>(bool debug = true)
        where T : IView
    {
        init();
        Blindness.App.Behaviour = new PamellaApp(Implementation);
        Blindness.App.Debug = debug;
        Blindness.App.StartWith<T>();
    }

    /// <summary>
    /// Add a view in view stack.
    /// </summary>
    public static void Push(IView view)
    {
        init();
        Implementation.Push(view);
    }

    /// <summary>
    /// Go back to previous view in view stack.
    /// </summary>
    public static void Pop()
    {
        init();
        Implementation.Pop();
    }

    /// <summary>
    /// Clear view stack.
    /// </summary>
    public static void Clear()
    {
        init();
        Implementation.Clear();
    }

    /// <summary>
    /// Close the app.
    /// </summary>
    public static void Close()
    {
        init();
        Implementation.Close();
    }
}