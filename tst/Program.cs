using System.Drawing;

using Pamella;

var view = new MainView();
App.Open(view);

public class MainView : View
{
    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.Black);
    }
}