/* Author:  Leonardo Trevisan Silio
 * Date:    29/03/2023
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Pamella;

using Providers;
using Providers.WindowsForms;

/// <summary>
/// Manage your app.
/// </summary>
public static class App
{
    private static GraphicsProvider provider = null;
    private static IGraphics graphics = null;
    private static void getGraphics(ProviderArgument args)
        => graphics = provider.Provide(args);

    static App()
    {
        provider = new GraphicsProvider();
        provider.Add(new WindowsFormsProvider());
    }
    
    // TODO: Send Winforms implementation to Providers abstarction
    private static Form mainForm = null;
    private static PictureBox pb = null;
    private static Bitmap bitmap = null;
    private static Graphics grap = null;
    private static View currView = null;
    private static Stack<View> stack = new Stack<View>();

    private static void configureApp()
    {
        if (mainForm is not null)
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
            
            WindowsFormsProviderArguments args = new WindowsFormsProviderArguments();
            args.Bitmap = bitmap;
            args.Graphics = grap;
            args.PictureBox = pb;
            getGraphics(args);
        };

        Application.Idle += delegate
        {
            var crr =
                stack.Count == 0 ?
                currView :
                stack.Peek();

            if (crr is null)
                return;
            
            crr.Draw(graphics);
            pb.Refresh();
        };

        Application.Run(mainForm);
    }

    /// <summary>
    /// Define a View as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
    /// <param name="view">The view to be displayed on the screen.</param>
    public static void Open(View view)
    {
        if (view is null)
            throw new ArgumentNullException("view");
        
        Clear();
        
        currView = view;
        configureApp();
    }

    /// <summary>
    /// Add a view in view stack.
    /// </summary>
    public static void Push(View view)
    {
        if (view is null)
            throw new ArgumentNullException("view");
        
        stack.Push(view);
    }

    /// <summary>
    /// Go back to previous view in view stack.
    /// </summary>
    public static void Pop()
    {
        if (stack.Count < 1)
            return;

        stack.Pop();
    }

    /// <summary>
    /// Clear view stack.
    /// </summary>
    public static void Clear()
        => stack.Clear();

    /// <summary>
    /// Close the app.
    /// </summary>
    public static void Close()
    {
        if (mainForm is null)
            throw new InvalidOperationException("The app dont started yet.");
        
        Application.Exit();
    }
}