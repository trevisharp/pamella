using Microsoft.CodeAnalysis;

App.Run<Home>(app =>
{
    app.Title = "Pamella playground";
    app.Size = (800, 600);
});

public interface Home : View
{
    [EagerLoading] dynamic Background { get; set; }

    void Design()
    {
        Background ??= shader((vec4 fillColor) =>
        {
            val axis = mix(x, y, sin(t) + 1);
            color = mix(fillColor, transparent, sin(0.001 * axis));
            fill();
        });
    }

    void Paint() => Background(Polygons.Screen, blue);

    void Tick(float dt) => Invalidate();
}
