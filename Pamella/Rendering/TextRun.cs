/* Author:  Leonardo Trevisan Silio
 * Date:    23/11/2026
 */
using System.Collections.Generic;

using Radiance.Fonts;
using Radiance.Bufferings;

namespace Pamella;

/// <summary>
/// Represents a glyph placed on a text, relative to the origin of the text.
/// </summary>
readonly record struct PlacedGlyph(Polygon Polygon, float X, float Y, Area Bounds);

/// <summary>
/// Represents a text as glyphs shared by every text with the same font and size.
/// </summary>
sealed class TextRun(PlacedGlyph[] glyphs)
{
    const int limit = 4096;
    static readonly Dictionary<(char, float, IFont), (Polygon Polygon, Area Bounds)?> shapes = [];

    public PlacedGlyph[] Glyphs => glyphs;

    /// <summary>
    /// Place the glyphs of a text as the font does on a single polygon.
    /// </summary>
    public static TextRun Create(string text, float size, IFont font)
    {
        var placed = new List<PlacedGlyph>();
        var lineHeight = font.LineHeight * size;
        var lines = text.Split('\n');
        for (int line = 0; line < lines.Length; line++)
        {
            var content = lines[line].TrimEnd('\r');
            var baseline = -line * lineHeight;
            var pen = 0f;
            for (int i = 0; i < content.Length; i++)
            {
                if (i > 0)
                    pen += font.MeasureWidth(content.Substring(i - 1, 2), size)
                        - font.MeasureWidth(content.Substring(i, 1), size);

                if (GetShape(content[i], size, font) is (Polygon polygon, Area bounds))
                    placed.Add(new PlacedGlyph(polygon, pen, baseline, bounds));
            }
        }

        return new TextRun([ ..placed ]);
    }

    static (Polygon, Area)? GetShape(char character, float size, IFont font)
    {
        if (shapes.TryGetValue((character, size, font), out var shape))
            return shape;

        if (shapes.Count > limit)
            shapes.Clear();

        var polygon = Polygon.FromText(font, character.ToString(), size);
        shape = polygon.Contours == 0 ? null : (polygon, Geometry.Bounds(polygon));
        shapes[(character, size, font)] = shape;
        return shape;
    }
}
