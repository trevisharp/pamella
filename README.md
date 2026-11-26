# Pamella

A library to create desktop apps in a beautiful way, rendered on the GPU.

# Table of Contents

 - [Overview](#overview)
 - [How to install](#how-to-install)
 - [Learn by examples](#learn-by-examples)
 - [Versions](#versions)

# Overview

Pamella is a replacement for Windows Forms where everything is drawn on the GPU. It is built over two libraries: [Radiance](https://github.com/trevisharp/radiance), for the OpenGL rendering and the shader DSL, and [Blindness](https://github.com/trevisharp/blindness), for dependency injection, binding, generated nodes and hot reload.

Components are interfaces. A source generator implements them, creates a constructor function for each one and keeps their state across hot reloads. You draw with shaders written in C#, and Pamella batches the calls of all components with the same shader into a few draw calls.

# How to install

```bash
dotnet new console # Create project
dotnet add package Pamella --prerelease # Install Pamella
```

# Learn by examples

### Create your first app

```cs
App.Run<Home>(app =>
{
    app.Title = "My app";
    app.Size = (800, 600);
});

public interface Home : View
{
    dynamic Background { get; set; }

    void Design()
    {
        Background ??= shader(() =>
        {
            color = (.2f, .4f, .9f, 1f);
            fill();
        });
    }

    void Paint() => Background(Rect(200, 100));
}
```

Pamella adds the global usings for `Radiance`, `Pamella` and the generated components, so no `using` is needed. Run with `dotnet run` and edit the code: hot reload applies the changes and keeps the state of the app.

### Understand the lifecycle

```cs
public interface Profile : View
{
    string Name { get; set; }

    void Design() { }              // resources: shaders, canvas and sub views
    void Behaviour() { }           // events of this view, runs again on hot reload
    async Task State() { }         // loads data once, never runs again on hot reload
    void Load() { }                // runs when the view enters the tree
    void Paint() { }               // records what is drawn
    void Tick(float dt) { }        // runs on each frame
    void Unload() { }              // runs when the view leaves the tree
}
```

Every member is optional. `Paint` only runs again when the view is invalidated.

### Create components with constructor functions

```cs
public interface Tag : View
{
    [Required] string Text { get; set; }
    [Default(16f)] float Size { get; set; }
    dynamic Face { get; set; }
    [Private] int Hovers { get; set; }

    void Design()
    {
        Face ??= shader((vec4 tagColor) =>
        {
            color = tagColor;
            fill();
        });
    }

    void Paint() => Face(Rect(Size * Text.Length, Size * 2), (1f, 1f, 1f, 1f));
}
```

Each property becomes an optional parameter of the generated function `Tag(...)`, including the shaders, so a parent can change the look of a child:

```cs
var tag = Tag("new", x: 10, y: 20, size: 24f);
var custom = Tag("hot", face: MyShader);
```

 - `[Required]` makes the parameter mandatory and puts it first.
 - `[Default(value)]` sets the value used when the argument is missing.
 - `[Private]` keeps the property out of the function.

### Bind values from the parent

```cs
public interface Counter : View
{
    int Clicks { get; set; }
    dynamic Content { get; set; }

    void Design()
    {
        Content ??= view(ctx => Tag(
            text: bind(() => $"{Clicks} clicks"),
            x: 20, y: 20));
    }

    void Behaviour() => OnMouseDown += button => Clicks++;

    void Paint() => Content();
}
```

`bind(() => Property)` links the two properties in both ways. Other expressions, like the interpolated string, are computed and flow from the parent to the child.

### Repaint only what changes

```cs
public interface Light : View
{
    bool On { get; set; }                  // repaints when it changes
    [Untracked] int Toggles { get; set; }  // changes without repainting
}
```

Every property invalidates the view when its value changes, also when it changes by a binding. Use `[Untracked]` for values that are not drawn and `Invalidate()` for changes that are not properties. Moving a view with `LocalX` and `LocalY` does not run `Paint` again.

### Draw children, clip and scroll

```cs
public interface Scroll : View
{
    float ScrollY { get; set; }
    View[] Controls { get; set; }

    void Paint()
    {
        Clip(0, 0, 200, 300);
        Offset(0, ScrollY);
        Paint(Controls);
    }

    Area Area => new(0, 0, 200, 300);
}
```

The order of the `Paint` calls is the order on the screen. `Area` sets the region that receives the mouse; without it, Pamella uses the bounds of what the view draws.

### Draw with a canvas

```cs
public interface Chart : View
{
    dynamic Art { get; set; }

    void Design()
    {
        Art ??= canvas((Graphics g, float size) =>
        {
            g.FillRectangle(new Vec4(1, 1, 0, 1), 0, 0, size, size);
            g.FillEllipse(new Vec4(0, 1, 1, 1), 40, 0, size, size);
            g.DrawLine(new Vec4(1, 0, 1, 1), 2, 0, 0, 80, 40);
            g.DrawString("Hello", 24, new Vec4(1, 1, 1, 1), 0, 60);
        });
    }

    void Paint() => Art(30f);
}
```

A cached canvas cannot capture variables: send the values as arguments.

### Handle events

```cs
public interface Field : View
{
    string Text { get; set; }

    void Behaviour()
    {
        OnMouseDown += button => Focus(this);
        OnTextInput += text => Text += text;
        OnKeyDown += (key, modifier) =>
        {
            if (key == Input.Backspace && Text.Length > 0)
                Text = Text[..^1];
        };
    }
}
```

Mouse events go to the view in front of the cursor and bubble to the parents until `StopPropagation()` is called. Keyboard events go to the focused view, and Tab moves the focus.

# Versions

### Pamella 3.0.0-preview1 (coming soon)

 - ![](https://img.shields.io/badge/update-blue) Recreate Pamella over Radiance 4.0 and Blindness 4.0, targeting net11.0.
 - ![](https://img.shields.io/badge/new-green) Components as interfaces with Design, Behaviour, State, Load, Paint, Tick and Unload.
 - ![](https://img.shields.io/badge/new-green) Generated constructor functions with optional parameters, bind arguments, Required, Default and Private.
 - ![](https://img.shields.io/badge/new-green) Shaders with the position of the view, reused by their GLSL and batched across components.
 - ![](https://img.shields.io/badge/new-green) Automatic invalidation of changed properties, with Untracked to opt out.
 - ![](https://img.shields.io/badge/new-green) Eager to create shared resources once on start.
 - ![](https://img.shields.io/badge/new-green) Canvas with Graphics, sub views, Clip, Offset and Area.
 - ![](https://img.shields.io/badge/new-green) Mouse and keyboard routing with bubbling, capture and focus.
 - ![](https://img.shields.io/badge/new-green) Hot reload that keeps the state of the components.
 - ![](https://img.shields.io/badge/removed-red) Stop using Stately lib.
 - ![](https://img.shields.io/badge/removed-red) Remove many structures used of old pamella version.

### Pamella 1.7

- ![](https://img.shields.io/badge/new-green) The implementation of provider for other plataforms is more easyer now.
- ![](https://img.shields.io/badge/new-green) Image drawing in screen.
- ![](https://img.shields.io/badge/new-green) Add Subview-system.
- ![](https://img.shields.io/badge/new-green) Add view protected methods to make works more easyer.
- ![](https://img.shields.io/badge/new-green) Add a container object that a collection of views to create screens more faster.
- ![](https://img.shields.io/badge/new-green) Improve objects initialization alot.
- ![](https://img.shields.io/badge/new-green) Sprite abstratction added for games.
- ![](https://img.shields.io/badge/update-blue) OnRender it is no longer abstract, it is now virtual.
- ![](https://img.shields.io/badge/update-blue) Update version of Stately for version 1.3.
- ![](https://img.shields.io/badge/bug%20solved-orange) Bugs correction.
- ![](https://img.shields.io/badge/bug%20solved-orange) Solve bug of renderization of sub views of sub views.
