/* Author:  Leonardo Trevisan Silio
 * Date:    27/05/2023
 */
using System;
using System.Drawing;

namespace Pamella;

public static class GeometricExtensions
{
    public static PointF Center(this RectangleF rect)
    {
        var center = new PointF(
            rect.X + rect.Width / 2,
            rect.Y + rect.Height / 2
        );

        return center;
    }
}