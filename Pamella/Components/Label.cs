/* Author:  Leonardo Trevisan Silio
 * Date:    23/11/2026
 */
using Radiance.Fonts;
using static Radiance.Utils;

namespace Pamella;

using static Pamella.Utils;

public interface Label : View
{
    [Default("")] string Text { get; set; }
    [Default(16f)] float Size { get; set; }
    float Width { get; set; }
    IFont Font { get; set; }
    [EagerLoading] dynamic TextShader { get; set; }

    void Design()
    {
        TextShader ??= shader(() =>
        {
            color = black;
            fill();
        });
    }

    void Paint()
    {
        TextShader(text(Text, Size, Font, Width > 0 ? Width : float.PositiveInfinity));
    }
}