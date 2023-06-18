/* Author:  Leonardo Trevisan Silio
 * Date:    13/06/2023
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Pamella.Providers.WindowsForms;

public class WindowsFormsApp : IApp
{
    private IGraphics graphics = null;
    private ProviderNode provider = null;

    public WindowsFormsApp(ProviderNode node)
        => this.provider = node;
    
    private Form mainForm = null;
    private PictureBox pb = null;
    private Bitmap bmp = null;
    private Graphics graph = null;
    private View currView = null;
    private bool running = false;
    private Stack<View> stack = new Stack<View>();

    private void configureApp()
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
            bmp = new Bitmap(pb.Width, pb.Height);
            graph = Graphics.FromImage(bmp);

            graph.Clear(Color.White);
            pb.Image = bmp;
            
            var args = new WindowsFormsProviderArguments();
            args.Bitmap = bmp;
            args.Graphics = graph;
            args.PictureBox = pb;
            args.Form = mainForm;
            this.graphics = new WindowsFormsGraphics(args);
        };

        running = true;
        EventHandler loop = delegate
        {
            while (running)
            {
                var crr =
                    stack.Count == 0 ?
                    currView :
                    stack.Peek();

                if (crr is null)
                    return;
                
                crr.Draw(graphics);
                pb.Refresh();

                Application.DoEvents();
            }
        };

        mainForm.FormClosing += delegate
        {
            running = false;
            Application.Idle -= loop;
            Application.Exit();
        };

        Application.Idle += loop;

        Application.Run(mainForm);
    }

    /// <summary>
    /// Define a View as a current view.
    /// <br>Clear the current stack if it exits.</br>
    /// </summary>
    /// <param name="view">The view to be displayed on the screen.</param>
    public void Open(View view)
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
    public void Push(View view)
    {
        if (view is null)
            throw new ArgumentNullException("view");
        
        stack.Push(view);
    }

    /// <summary>
    /// Go back to previous view in view stack.
    /// </summary>
    public void Pop()
    {
        if (stack.Count < 1)
            return;

        stack.Pop();
    }

    /// <summary>
    /// Clear view stack.
    /// </summary>
    public void Clear()
        => stack.Clear();

    /// <summary>
    /// Close the app.
    /// </summary>
    public void Close()
    {
        if (mainForm is null)
            throw new InvalidOperationException("The app dont started yet.");
        
        running = false;
        Application.Exit();
    }
}