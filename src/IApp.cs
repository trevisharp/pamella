/* Author:  Leonardo Trevisan Silio
 * Date:    03/04/2023
 */
namespace Pamella;

public interface IApp
{
    /// <summary>
    /// Define a View as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
    /// <param name="view">The view to be displayed on the screen.</param>
    void Open(View view);

    /// <summary>
    /// Add a view in view stack.
    /// </summary>
    void Push(View view);

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