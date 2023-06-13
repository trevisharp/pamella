/* Author:  Leonardo Trevisan Silio
 * Date:    13/06/2023
 */
using System;
using System.Drawing;

namespace Pamella;

using Views;

partial class View
{
    /// <summary>
    /// Create a new button.
    /// </summary>
    protected Button button(
        ref Button obj, 
        Color? color = null,
        Color? selectedColor = null,
        Color? pressedColor = null,
        string? text = null,
        RectangleF? rect = null,
        Color? borderColor = null,
        Color? selectedBorderColor = null,
        Color? pressedBorderColor = null,
        float? borderWidth = null,
        int? cornerRadius = null,
        Action<Button, PointF> onMouseDown = null,
        Action<Button, PointF> onMouseUp = null
    )
    {
        var newButton = new Button
        {
            Color = color ?? Color.White,
            SelectedBorderColor = selectedBorderColor ?? Color.Black,
            PressedColor = pressedColor ?? Color.White,
            SelectedColor = selectedColor ?? Color.White,
            Text = text ?? "",
            Rect = rect ?? new RectangleF(50, 50, 100, 100),
            BorderColor = borderColor ?? Color.Black,
            BorderWidth = borderWidth ?? 4,
            CornerRadius = cornerRadius ?? 10,
            OnMouseDown = onMouseDown,
            OnMouseUp = onMouseUp
        };

        obj = newButton;
        return newButton;
    }

    /// <summary>
    /// Create a new button.
    /// </summary>
    protected Button button(
        Color? color = null,
        Color? selectedColor = null,
        Color? pressedColor = null,
        string? text = null,
        RectangleF? rect = null,
        Color? borderColor = null,
        Color? selectedBorderColor = null,
        Color? pressedBorderColor = null,
        float? borderWidth = null,
        int? cornerRadius = null,
        Action<Button, PointF> onMouseDown = null,
        Action<Button, PointF> onMouseUp = null
    )
    {
        var newButton = new Button
        {
            Color = color ?? Color.White,
            SelectedBorderColor = selectedBorderColor ?? Color.Black,
            PressedColor = pressedColor ?? Color.White,
            SelectedColor = selectedColor ?? Color.White,
            Text = text ?? "",
            Rect = rect ?? new RectangleF(50, 50, 100, 100),
            BorderColor = borderColor ?? Color.Black,
            BorderWidth = borderWidth ?? 4,
            CornerRadius = cornerRadius ?? 10,
            OnMouseDown = onMouseDown,
            OnMouseUp = onMouseUp
        };

        return newButton;
    }
}