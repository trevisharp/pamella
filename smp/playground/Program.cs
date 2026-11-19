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
            color = fillColor;
            fill();
        });
    }

    void Paint() => Background(Rect(200, 100), blue);
}
