App.Run<Home>(app =>
{
    app.Title = "Pamella playground";
    app.Size = (800, 600);
});

public interface Home : View
{
    string ButtonText { get; set; }
    dynamic Views { get; set; }

    [Private] dynamic BtShader1 { get; set; }
    [Private] dynamic BtShader2 { get; set; }
    [Private] bool BtSelected { get; set; }
    [Private] float BtX { get; set; }
    [Private] float BtY { get; set; }
    
    void Design()
    {
        BtShader1 ??= shader(() =>
        {
            color = white;
            fill();

            color = black;
            draw(2f);
        });

        BtShader2 ??= shader(() =>
        {
            color = black;
            fill();

            color = white;
            draw(1f);
        });

        Views ??= view(ctx =>
            Button(
                x: bind(() => BtX),
                y: bind(() => BtY),
                text: bind(() => ButtonText),
                selected: bind(() => BtSelected),
                backgroundShader: bind(() => BtSelected ? BtShader2 : BtShader1)
            )
        );
    }

    void Behaviour()
    {
        OnMouseMove += p =>
        {
            if (!BtSelected)
                return;
            
            BtX = p.x;
            BtY = p.y;
        };
    }

    void Load()
    {
        ButtonText = "Olá mundo!";
    }

    void Paint()
    {
        Views();
    }
}

interface Button : View
{
    string Text { get; set; }
    bool Selected { get; set; }

    dynamic TextShader { get; set; }
    dynamic BackgroundShader { get; set; }

    void Design()
    {
        TextShader ??= shader(() =>
        {
            color = black;
            fill();
        });

        BackgroundShader ??= shader(() =>
        {
            color = (.7f, .7f, .7f, 1f);
            fill();
        });
    }

    void Behaviour()
    {
        (float x, float y) cursor = (0, 0);
        OnMouseMove += p => cursor = p;

        OnMouseDown += bt =>
        {
            if (bt != MouseButton.Left)
                return;
            
            Selected = true;
        };

        OnMouseUp += bt =>
        {
            if (bt != MouseButton.Left)
                return;
            
            Selected = false;
        };

        OnMouseLeave += () =>
        {
            Selected = false;
        };
    }

    void Paint()
    {
        BackgroundShader(Rect(80, 40));
        TextShader(Text);
    }
}