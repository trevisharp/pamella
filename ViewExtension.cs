/* Author:  Leonardo Trevisan Silio
 * Date:    24/05/2023
 */
using System;
using System.Drawing;

namespace Pamella;

using Views;

partial class View
{
    /// <summary>
    /// Get a random float number in the [0, 1) range.
    /// </summary>
    protected float rand()
    {
        var rand = Random.Shared;
        return rand.NextSingle();
    }

    /// <summary>
    /// Get a random integer number in the [0, N - 1] range.
    /// </summary>
    protected int rand(int N)
    {
        var rand = Random.Shared;
        return rand.Next(N);
    }

    /// <summary>
    /// Select a random element in a array.
    /// </summary>
    /// <param name="elements">Array elements of type T.</param>
    /// <returns>A single object of the array.</returns>
    protected T choose<T>(params T[] elements)
    {
        if (elements is null)
            return default(T);

        int len = elements.Length;
        if (len == 0)
            return default(T);
        
        var rand = Random.Shared;
        var index = rand.Next(len);
        return elements[index];
    }

    /// <summary>
    /// Create a rectangle in position x, y with a width and heigth.
    /// </summary>
    protected RectangleF rect(float x, float y, float width, float height)
        => new RectangleF(x, y, width, height);
}