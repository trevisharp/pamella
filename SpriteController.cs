/* Author:  Leonardo Trevisan Silio
 * Date:    27/05/2023
 */
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Pamella;

/// <summary>
/// A controller system for sprites.
/// </summary>
public class SpriteController
{
    private int index = 0;
    private double fps = 40;
    private DateTime last = DateTime.Now;
    private TimeSpan frameTime = TimeSpan.FromMilliseconds(25);

    public Bitmap Image { get; set; }
    public SizeF BaseSize { get; set; }
    public List<PointF> FramePoints { get; set; } = new List<PointF>();
    public double FramePerSeconds
    {
        get => this.fps;
        set
        {
            this.fps = value;
            this.frameTime = TimeSpan.FromMilliseconds(25);
        }
    }
    
    public void AddPoints(PointF pt)
        => this.FramePoints.Add(pt);
    
    public PointF AddPoints(PointF pt, Size desloc, int count)
    {
        this.AddPoints(pt);

        for (int i = 1; i < count; i++)
        {
            pt += desloc;
            this.AddPoints(pt);
        }

        return pt;
    }

    public void Draw(IGraphics g, RectangleF screenRect)
    {
        var now = DateTime.Now;
        var timePassed = now - last;
        if (timePassed > frameTime)
        {
            this.last = now;
            this.index++;
            if (this.index >= FramePoints.Count)
                this.index = 0;
        }
        var crrPoint = FramePoints[index];
        var rect = new RectangleF(crrPoint, BaseSize);
        g.DrawImage(screenRect, Image, rect);
    }
    
    public static SpriteController Load(Bitmap bmp, Size size,
        PointF initial, Size? desloc = null, int count = 0
    )
    {
        SpriteController sprite = new SpriteController();

        sprite.Image = bmp;
        sprite.BaseSize = size;
        if (desloc is not null)
            sprite.AddPoints(initial, desloc.Value, count);
        else sprite.AddPoints(initial);

        return sprite;
    }

    public static SpriteController Load(string path, Size size, 
        PointF initial, Size? desloc = null, int count = 0
    )
    {
        SpriteController sprite = new SpriteController();

        sprite.Image = Bitmap.FromFile(path) as Bitmap;
        sprite.BaseSize = size;
        if (desloc is not null)
            sprite.AddPoints(initial, desloc.Value, count);
        else sprite.AddPoints(initial);

        return sprite;
    }

    public static SpriteController Load(Bitmap bmp, SizeF size)
    {
        SpriteController sprite = new SpriteController();

        sprite.Image = bmp;
        sprite.BaseSize = size;

        return sprite;
    }

    public static SpriteController Load(string path, SizeF size)
    {
        SpriteController sprite = new SpriteController();

        sprite.Image = Bitmap.FromFile(path) as Bitmap;
        sprite.BaseSize = size;

        return sprite;
    }

    public static SpriteController Load(Bitmap bmp)
    {
        SpriteController sprite = new SpriteController();

        sprite.Image = bmp;
        sprite.BaseSize = bmp.Size;

        return sprite;
    }

    public static SpriteController Load(string path)
    {
        SpriteController sprite = new SpriteController();

        var bmp = Bitmap.FromFile(path) as Bitmap;
        sprite.Image = Bitmap.FromFile(path) as Bitmap;
        sprite.BaseSize = bmp.Size;

        return sprite;
    }
}