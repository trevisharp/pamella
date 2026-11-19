/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;

using Radiance;

namespace Pamella;

/// <summary>
/// Stores the event handlers registered by a view.
/// </summary>
sealed class ViewEvents
{
    public Action<Input, Modifier> KeyDown;
    public Action<Input, Modifier> KeyUp;
    public Action<(float x, float y)> MouseMove;
    public Action<MouseButton> MouseDown;
    public Action<MouseButton> MouseUp;
    public Action<float> MouseWheel;
    public Action MouseEnter;
    public Action MouseLeave;
    public Action<float, float> Resize;
    public Action<string> TextInput;
    public Action<bool> FocusChange;
    public Action<string[]> FileDrop;

    public bool Focusable => KeyDown is not null || KeyUp is not null || TextInput is not null;
}
