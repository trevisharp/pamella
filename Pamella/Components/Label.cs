/* Author:  Leonardo Trevisan Silio
 * Date:    23/11/2026
 */
using Radiance.Fonts;

namespace Pamella;

using static Pamella.Utils;

public interface Label : View
{
    [Default("text")] string Text { get; set; }
    [Default(16f)] float Size { get; set; }
    float MaxWidth { get; set; }
    IFont Font { get; set; }
    [Eager] dynamic TextShader { get; set; }

    void Design()
    {
        TextShader ??= shader(() =>
        {
            color = black;
            fill();
        });
    }

    TextRun Content => text(
        Text, Size, Font,
        MaxWidth > 0 ? MaxWidth : float.PositiveInfinity
    );

    Area Area => Content.Bounds;

    void Paint() => TextShader(Content);
}