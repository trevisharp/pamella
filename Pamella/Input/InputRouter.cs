/* Author:  Leonardo Trevisan Silio
 * Date:    18/11/2026
 */
using System;
using System.Linq;
using System.Collections.Generic;

using Radiance;

namespace Pamella;

/// <summary>
/// Sends the events of the window to the views.
/// </summary>
static class InputRouter
{
    static readonly HashSet<ViewNode> hovered = [];
    static float mouseX, mouseY;
    static ViewNode captured;
    static ViewNode focused;
    static bool stopped;

    public static void Attach()
    {
        Window.OnMouseMove += OnMouseMove;
        Window.OnMouseDown += OnMouseDown;
        Window.OnMouseUp += button =>
        {
            Bubble(Target(), e => e.MouseUp?.Invoke(button));
            captured = null;
        };
        Window.OnMouseWheel += delta => Bubble(Target(), e => e.MouseWheel?.Invoke(delta));
        Window.OnMouseLeave += () => UpdateHover(null);
        Window.OnKeyDown += OnKeyDown;
        Window.OnKeyUp += (key, modifier) => Bubble(focused, e => e.KeyUp?.Invoke(key, modifier));
        Window.OnTextInput += text => Bubble(focused, e => e.TextInput?.Invoke(text));
        Window.OnFileDrop += files => Bubble(Target(), e => e.FileDrop?.Invoke(files));
        Window.OnFocusChange += value =>
        {
            if (focused?.Events is ViewEvents events)
                Run(focused, () => events.FocusChange?.Invoke(value));
        };
        Window.OnResize += (width, height) =>
        {
            Scene.MarkComposition();
            if (Scene.Root?.Events is ViewEvents events)
                Run(Scene.Root, () => events.Resize?.Invoke(width, height));
            Scene.Root?.Invalidate();
        };
    }

    /// <summary>
    /// Stop sending the current event to the parents.
    /// </summary>
    public static void StopPropagation() => stopped = true;

    /// <summary>
    /// Give the keyboard focus to a view.
    /// </summary>
    public static void Focus(ViewNode view)
    {
        if (focused == view)
            return;

        var previous = focused;
        focused = view;
        if (previous?.Events is ViewEvents oldEvents)
            Run(previous, () => oldEvents.FocusChange?.Invoke(false));
        if (view?.Events is ViewEvents newEvents)
            Run(view, () => newEvents.FocusChange?.Invoke(true));
    }

    static ViewNode Target() => captured ?? Hit(mouseX, mouseY);

    static ViewNode Hit(float x, float y)
    {
        var regions = Scene.Regions;
        for (int i = regions.Count - 1; i >= 0; i--)
        {
            var region = regions[i];
            if (region.View.Mounted && region.Area.Contains(x, y)
                && (region.Scissor is not Scissor limit || limit.Area.Contains(x, y)))
                return region.View;
        }

        return null;
    }

    static void OnMouseMove((float x, float y) position)
    {
        (mouseX, mouseY) = position;
        var target = Target();
        UpdateHover(target);

        stopped = false;
        for (var view = target; view is not null && !stopped; view = view.ParentNode)
        {
            if (view.Events?.MouseMove is not Action<(float x, float y)> handler)
                continue;

            var local = (position.x - view.GlobalX, position.y - view.GlobalY);
            Run(view, () => handler(local));
        }
    }

    static void OnMouseDown(MouseButton button)
    {
        var target = Hit(mouseX, mouseY);
        captured = target;

        var focusable = target;
        while (focusable is not null && focusable.Events?.Focusable != true)
            focusable = focusable.ParentNode;
        Focus(focusable);

        Bubble(target, e => e.MouseDown?.Invoke(button));
    }

    static void OnKeyDown(Input key, Modifier modifier)
    {
        if (key == Input.Tab)
        {
            MoveFocus(modifier.HasFlag(Modifier.Shift) ? -1 : 1);
            return;
        }

        Bubble(focused, e => e.KeyDown?.Invoke(key, modifier));
    }

    static void MoveFocus(int direction)
    {
        var views = Scene.Regions
            .Select(region => region.View)
            .Where(view => view.Events?.Focusable == true)
            .Distinct()
            .ToList();
        if (views.Count == 0)
            return;

        var index = views.IndexOf(focused);
        var next = index < 0
            ? 0
            : (index + direction + views.Count) % views.Count;
        Focus(views[next]);
    }

    static void UpdateHover(ViewNode target)
    {
        HashSet<ViewNode> current = [];
        for (var view = target; view is not null; view = view.ParentNode)
            current.Add(view);

        foreach (var view in hovered.Where(v => !current.Contains(v)).ToArray())
        {
            hovered.Remove(view);
            if (view.Events is ViewEvents events)
                Run(view, () => events.MouseLeave?.Invoke());
        }

        foreach (var view in current.Reverse())
        {
            if (hovered.Add(view) && view.Events is ViewEvents events)
                Run(view, () => events.MouseEnter?.Invoke());
        }
    }

    static void Bubble(ViewNode target, Action<ViewEvents> send)
    {
        stopped = false;
        for (var view = target; view is not null && !stopped; view = view.ParentNode)
        {
            if (view.Events is ViewEvents events)
                Run(view, () => send(events));
        }
    }

    static void Run(ViewNode view, Action action)
        => Scene.Guard(view, action);
}
