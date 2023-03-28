/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pamella;

/// <summary>
/// Represents a visualization for the app
/// </summary>
public abstract class View
{
    public virtual void Draw(Graphics g) { }
}