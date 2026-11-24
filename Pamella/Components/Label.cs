/* Author:  Leonardo Trevisan Silio
 * Date:    23/11/2026
 */
using static Radiance.Utils;

namespace Pamella;

using static Pamella.Utils;

public interface Label : View
{
    [Default("")] string Text { get; set; }
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
        TextShader(Text);
    }
}