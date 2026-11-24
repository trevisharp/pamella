/* Author:  Leonardo Trevisan Silio
 * Date:    23/11/2026
 */
using System.Text;
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
public sealed class TextRun
{
    const int limit = 4096;
    static readonly Dictionary<(char, float, IFont), (Polygon Polygon, Area Bounds)?> shapes = [];

    TextRun(PlacedGlyph[] glyphs) => Glyphs = glyphs;

    internal PlacedGlyph[] Glyphs { get; }

    /// <summary>
    /// Place the glyphs of a text as the font does on a single polygon.
    /// </summary>
    internal static TextRun Create(string text, float size, IFont font, float maxWidth)
    {
        var placed = new List<PlacedGlyph>();
        var lineHeight = font.LineHeight * size;
        var lines = BreakLines(text, size, font, maxWidth);
        for (int line = 0; line < lines.Count; line++)
        {
            var content = lines[line];
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

    static List<string> BreakLines(string text, float size, IFont font, float maxWidth)
    {
        var lines = new List<string>();
        foreach (var paragraph in text.Split('\n'))
        {
            if (float.IsInfinity(maxWidth) || font.MeasureWidth(paragraph, size) <= maxWidth)
            {
                lines.Add(paragraph.TrimEnd('\r'));
                continue;
            }

            var line = new StringBuilder();
            foreach (var word in paragraph.TrimEnd('\r').Split(' '))
            {
                if (line.Length == 0)
                {
                    line.Append(word);
                    continue;
                }

                var candidate = $"{line} {word}";
                if (font.MeasureWidth(candidate, size) <= maxWidth)
                {
                    line.Clear();
                    line.Append(candidate);
                    continue;
                }

                lines.Add(line.ToString());
                line.Clear();
                line.Append(word);
            }

            lines.Add(line.ToString());
        }

        return lines;
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
