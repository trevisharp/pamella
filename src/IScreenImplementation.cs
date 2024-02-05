/* Author:  Leonardo Trevisan Silio
 * Date:    04/02/2024
 */
namespace Pamella;

/// <summary>
/// Represents a screen engine implementation.
/// </summary>
public interface IScreenImplementation
{
    /// <summary>
    /// Define a View as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
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