using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public sealed class TextBox : Control
{
    public event EventHandler<GuiValueChangedEventArgs<string>>? OnTextChanged;

    private int cursorIndex;

    public string Text { get; private set; } = "";
    public int MaxLength { get; set; } = 256;
    public string Placeholder { get; set; } = "";
    public double CursorBlinkSeconds { get; set; } = 0.5;

    public TextBox()
    {
        Focusable = true;
        PreferredSize = new System.Numerics.Vector2(180, 36);
    }

    public TextBox(string text)
        : this()
    {
        Text = text;
    }

    public void SetText(string value)
    {
        value ??= "";

        if (value.Length > MaxLength)
            value = value[..MaxLength];

        if (Text == value)
            return;

        Text = value;
        cursorIndex = Math.Clamp(cursorIndex, 0, Text.Length);
        OnTextChanged?.Invoke(this, new GuiValueChangedEventArgs<string>(Text));
    }

    protected override void HandleTextInput(GuiTextInputEventArgs args)
    {
        if (Text.Length >= MaxLength || args.Codepoint < 0 || args.Codepoint > 0x10FFFF)
            return;

        if (args.Codepoint <= char.MaxValue && char.IsControl((char)args.Codepoint))
            return;

        string input = char.ConvertFromUtf32(args.Codepoint);
        int nextCursor = cursorIndex + input.Length;
        SetText(Text.Insert(cursorIndex, input));
        cursorIndex = nextCursor;
    }

    protected override void HandleKeyDown(GuiKeyEventArgs args)
    {
        switch (args.Key)
        {
            case KeyboardKey.Backspace when cursorIndex > 0:
                int backspaceCursor = cursorIndex - 1;
                SetText(Text.Remove(cursorIndex - 1, 1));
                cursorIndex = backspaceCursor;
                break;

            case KeyboardKey.Delete when cursorIndex < Text.Length:
                SetText(Text.Remove(cursorIndex, 1));
                break;

            case KeyboardKey.Left:
                cursorIndex = Math.Max(0, cursorIndex - 1);
                break;

            case KeyboardKey.Right:
                cursorIndex = Math.Min(Text.Length, cursorIndex + 1);
                break;

            case KeyboardKey.Home:
                cursorIndex = 0;
                break;

            case KeyboardKey.End:
                cursorIndex = Text.Length;
                break;
        }
    }

    public override void Draw()
    {
        int fontSize = ResolvedFontSize;
        string visibleText = Text.Length == 0 ? Placeholder : Text;
        Color textColor = Text.Length == 0 ? Theme.MutedText : ResolvedTextColor;

        DrawBox(BoundingBox, Enabled ? Theme.Surface : Theme.Disabled, IsFocused ? Theme.Focus : Theme.Border);

        int x = (int)BoundingBox.X + Theme.Padding;
        int y = (int)(BoundingBox.Y + (BoundingBox.Height - fontSize) / 2);
        Raylib.DrawText(visibleText, x, y, fontSize, textColor);

        if (IsFocused && ShouldDrawCursor())
        {
            string beforeCursor = Text[..cursorIndex];
            int width = Raylib.MeasureText(beforeCursor, fontSize);
            int cursorX = x + width + 1;
            int top = y;
            Raylib.DrawLine(cursorX, top, cursorX, top + fontSize, Theme.Text);
        }
    }

    private bool ShouldDrawCursor()
    {
        if (CursorBlinkSeconds <= 0)
            return true;

        return Math.Floor(Raylib.GetTime() / CursorBlinkSeconds) % 2 == 0;
    }
}
