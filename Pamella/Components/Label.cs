/* Author:  Leonardo Trevisan Silio
 * Date:    23/11/2026
 */
using Radiance.Bufferings;
using static Radiance.Utils;

namespace Pamella;

using static Pamella.Utils;

public interface Label : View
{
    string Text { get; set; }

    [Private]
    Polygon TextBuffer { get; set; }
    dynamic TextShader { get; set; }

    void Design()
    {
        TextShader ??= shader(() =>
        {
            color = black;
            fill();
        });
    }

    void Behaviour()
    {
        Bind(() => TextBuffer == Polygon.FromText(Text));
    }

    void Paint()
    {
        TextShader(TextBuffer);
    }
}