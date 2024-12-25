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
    /// Open windown screen.
    /// </summary>
    void Open();

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