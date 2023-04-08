/* Author:  Leonardo Trevisan Silio
 * Date:    08/04/2023
 */
using Stately;
using System.Drawing;

namespace Pamella.Views;

/// <summary>
/// State for a Button view.
/// </summary>
public class ButtonState : State
{
    public Property<string> Text { get; set; }
    public Property<RectangleF> Rect { get; set; }
    public Property<bool> Selected { get; set; }
    public Property<bool> Pressed { get; set; }
    public Property<Color> BackColor { get; set; }
    public Property<Color> SelectedColor { get; set; }
    public Property<Color> PressedColor { get; set; }
    public Property<float> CornerRadius { get; set; }
    public Property<bool> HasBorder { get; set; }
    public Property<float> BorderWidth { get; set; }
    public Property<Color> BorderColor { get; set; }
    public Property<Color> BorderSelectedColor { get; set; }
    public Property<Color> BorderPressedColor { get; set; }
}