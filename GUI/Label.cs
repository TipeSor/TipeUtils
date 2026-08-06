using System.Numerics;
using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public class Label : Control
{
    public string Text { get; set; } = "";
    public Vector2 TextOffset { get; set; }

    public Label()
    {
        Enabled = false;
        PreferredSize = new Vector2(120, 24);
    }

    public Label(string text)
        : this()
    {
        Text = text;
    }

    public override void Draw()
    {
        int fontSize = ResolvedFontSize;
        Vector2 position = new(BoundingBox.X + TextOffset.X, BoundingBox.Y + TextOffset.Y);
        Raylib.DrawText(Text, (int)position.X, (int)position.Y, fontSize, ResolvedTextColor);
    }
}
