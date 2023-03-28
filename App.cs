/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Pamella;

/// <summary>
/// Manage your app
/// </summary>
public class App
{
    private static Form mainForm = null;
    private static PictureBox pb = null;
    private static Bitmap bitmap = null;
    private static Graphics grap = null;
    private static View currView = null;
    private static Stack<View> stack = new Stack<View>();

    private static void configureApp()
    {
        if (mainForm != null)
            return;
        
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        mainForm = new Form();
        mainForm.FormBorderStyle = FormBorderStyle.None;
        mainForm.WindowState = FormWindowState.Maximized;

        pb = new PictureBox();
        pb.Dock = DockStyle.Fill;
        mainForm.Controls.Add(pb);

        mainForm.Load += delegate
        {
            bitmap = new Bitmap(pb.Width, pb.Height);
            grap = Graphics.FromImage(bitmap);
            grap.Clear(Color.White);
            pb.Image = bitmap;
        };

        Application.Idle += delegate
        {
            if (currView == null)
                return;
            
            currView.Draw(grap);
        };

        Application.Run(mainForm);
    }

    /// <summary>
    /// Define a View as a current view and add her to view stack
    /// </summary>
    /// <param name="view">The view to be displayed on the screen.</param>
    public static void Open(View view)
    {
        if (view == null)
            throw new ArgumentNullException("view");
        
        stack.Push(view);
        currView = view;

        configureApp();
    }

    /// <summary>
    /// Go back to previous view in view stack.
    /// </summary>
    public static void Back()
    {
        if (stack.Count < 2)
        {
            Close();
            return;
        }

        stack.Pop();
        currView = stack.Peek();
    }

    /// <summary>
    /// Close the app.
    /// </summary>
    public static void Close()
    {
        if (mainForm == null)
            throw new InvalidOperationException("The app dont started yet.");
        
        Application.Exit();
    }
}