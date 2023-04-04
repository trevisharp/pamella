/* Author:  Leonardo Trevisan Silio
 * Date:    03/04/2023
 */
using System.Drawing;
using System.Windows.Forms;

namespace Pamella.Providers.WindowsForms;

/// <summary>
/// Provider Arguments for WindowsForms App
/// </summary>
public class WindowsFormsProviderArguments
{
    public Graphics Graphics { get; set; }
    public Bitmap Bitmap { get; set; }
    public PictureBox PictureBox { get; set; }
    public Form Form { get; set; }
}