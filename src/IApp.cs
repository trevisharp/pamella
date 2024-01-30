/* Author:  Leonardo Trevisan Silio
 * Date:    30/01/2024
 */
namespace Pamella;

/// <summary>
/// Represents a App engine implementation.
/// </summary>
public interface IApp
{
    /// <summary>
    /// Define a View as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
    /// <param name="view">The view to be displayed on the screen.</param>
    void Open(IView view);

    /// <summary>
    /// Add a view in view stack.
    /// </summary>
    void Push(IView view);

    /// <summary>
    /// Go back to previous view in view stack.
    /// </summary>
    void Pop();

    /// <summary>
    /// Clear view stack.
    /// </summary>
    void Clear();

    /// <summary>
    /// Close the app.
    /// </summary>
    void Close();
}