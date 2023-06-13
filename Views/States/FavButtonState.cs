/* Author:  Leonardo Trevisan Silio
 * Date:    17/05/2023
 */
using Stately;
using System.Drawing;

namespace Pamella.Views.States;

/// <summary>
/// State for a FavButton view.
/// </summary>
internal class FavButtonState : State
{
    public Property<Color> Color { get; set; }
    public Property<RectangleF> Rect { get; set; }
    public Property<float> AnimationSpeed { get; set; }
    public Property<float> AnimationSize { get; set; }
    public Property<bool> Checked { get; set; }
    public Property<bool> Selected { get; set; }
}