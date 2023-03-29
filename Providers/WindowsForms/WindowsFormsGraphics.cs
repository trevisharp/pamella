/* Author:  Leonardo Trevisan Silio
 * Date:    28/03/2023
 */
using System.Drawing;
using System.Windows.Forms;

namespace Pamella.Providers.WindowsForms;

/// <summary>
/// A WindowsForms IGraphics Implementatino
/// </summary>
public class WindowsFormsGraphics : IGraphics
{
    private Graphics g;
    private PictureBox pb;
    private Bitmap bmp;
    private PointF cursor;
    private bool isDown;
    private bool isRightDown;

    public WindowsFormsGraphics(WindowsFormsProviderArguments args)
    {
        this.g = args.Graphics;
        this.pb = args.PictureBox;
        this.bmp = args.Bitmap;
        this.cursor = PointF.Empty;
        this.isDown = false;

        pb.MouseMove += (o, e) =>
        {
            this.cursor = e.Location;
        };

        pb.MouseDown += (o, e) =>
        {
            if (e.Button == MouseButtons.Left)
                this.isDown = true;
            
            if (e.Button == MouseButtons.Right)
                this.isRightDown = true;
        };

        pb.MouseUp += (o, e) =>
        {
            if (e.Button == MouseButtons.Left)
                this.isDown = false;
            
            if (e.Button == MouseButtons.Right)
                this.isRightDown = false;
        };
    }

    public int Width
        => this.bmp.Width;

    public int Height
        => this.bmp.Height;

    public PointF Cursor
        => this.cursor;

    public bool IsDown
        => this.isDown;

    public bool IsRightDown
        => this.isRightDown;

    public void Clear(Color color)
        => g.Clear(color);

    public void DrawRectangle(RectangleF rect, Pen pen)
        => g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

    public void DrawRectangle(PointF loc, Size siz, Pen pen)
        => g.DrawRectangle(pen, loc.X, loc.Y, siz.Width, siz.Height);

    public void DrawRectangle(float x, float y, float width, float height, Pen pen)
        => g.DrawRectangle(pen, x, y, width, height);

    public void DrawText(RectangleF rect, string text)
        => g.DrawString(text, SystemFonts.MenuFont, Brushes.Black, rect);

    public void DrawText(RectangleF rect, StringAlignment horAlig, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        g.DrawString(text, SystemFonts.MenuFont, Brushes.Black, rect);
    }

    public void DrawText(RectangleF rect, StringAlignment horAlig, StringAlignment verAlig, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        format.LineAlignment = verAlig;
        g.DrawString(text, SystemFonts.MenuFont, Brushes.Black, rect);
    }

    public void DrawText(RectangleF rect, Font font, string text)
        => g.DrawString(text, font, Brushes.Black, rect);

    public void DrawText(RectangleF rect, Font font, StringAlignment horAlig, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        g.DrawString(text, font, Brushes.Black, rect);
    }

    public void DrawText(RectangleF rect, Font font, StringAlignment horAlig, StringAlignment verAlig, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        format.LineAlignment = verAlig;
        g.DrawString(text, font, Brushes.Black, rect);
    }

    public void DrawText(RectangleF rect, Brush brush, string text)
        => g.DrawString(text, SystemFonts.MenuFont, brush, rect);

    public void DrawText(RectangleF rect, StringAlignment horAlig, Brush brush, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        g.DrawString(text, SystemFonts.MenuFont, brush, rect);
    }

    public void DrawText(RectangleF rect, StringAlignment horAlig, StringAlignment verAlig, Brush brush, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        format.LineAlignment = verAlig;
        g.DrawString(text, SystemFonts.MenuFont, brush, rect);
    }

    public void DrawText(RectangleF rect, Font font, Brush brush, string text)
        => g.DrawString(text, font, brush, rect);

    public void DrawText(RectangleF rect, Font font, StringAlignment horAlig, Brush brush, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        g.DrawString(text, font, brush, rect);
    }

    public void DrawText(RectangleF rect, Font font, StringAlignment horAlig, StringAlignment verAlig, Brush brush, string text)
    {
        StringFormat format = new StringFormat();
        format.Alignment = horAlig;
        format.LineAlignment = verAlig;
        g.DrawString(text, font, brush, rect);
    }

    public void FillRectangle(RectangleF rect, Brush brush)
        => g.FillRectangle(brush, rect);

    public void FillRectangle(PointF loc, Size siz, Brush brush)
        => g.FillRectangle(brush, loc.X, loc.Y, siz.Width, siz.Height);

    public void FillRectangle(float x, float y, float width, float height, Brush brush)
        => g.FillRectangle(brush, x, y, width, height);
}