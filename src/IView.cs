/* Author:  Leonardo Trevisan Silio
 * Date:    31/12/2023
 */
using Radiance;
using Blindness;

namespace Pamella;

/// <summary>
/// Base type for all views in app.
/// </summary>
[Ignore]
public interface IView : INode
{
    void Draw();
    void OnFrame();
    void OnMouseDown(MouseButton button);
    void OnMouseMove(float x, float y);
    void OnMouseUp(MouseButton button);
    void INode.Run() => OnFrame();
}