using System.Numerics;
using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public class Button : Control
{
    public string Text { get; set; } = "Button";

    public Button()
    {
        Focusable = true;
        PreferredSize = new Vector2(120, 36);
    }

    public Button(string text)
        : this()
    {
        Text = text;
    }

    protected override void HandleKeyDown(GuiKeyEventArgs args)
    {
        if (args.Key is KeyboardKey.Enter or KeyboardKey.Space)
            DispatchClick(MouseButton.Left, Center);
    }

    public override void Draw()
    {
        DrawBackground();
        DrawCenteredText(Text, ResolvedTextColor, ResolvedFontSize);
    }

    protected Vector2 Center => new(BoundingBox.X + BoundingBox.Width / 2, BoundingBox.Y + BoundingBox.Height / 2);
}
