/* Author:  Leonardo Trevisan Silio
 * Date:    23/11/2026
 */
using Radiance.Bufferings;
using Radiance.Fonts;

namespace Pamella;

using static Pamella.Utils;

public interface Card : View
{
    [Eager] dynamic Shader { get; set; }
    [Eager] Polygon Shape { get; set; }
    [Default(5)] float Radius { get; set; }
    [Default(60)] float Width { get; set; } 
    [Default(40)] float Height { get; set; } 

    void Design()
    {
        Bind(() => Shape == Polygons.Polar(t => t));
    }
}