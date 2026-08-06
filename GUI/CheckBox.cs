using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public sealed class CheckBox : Control
{
    public event EventHandler<GuiValueChangedEventArgs<bool>>? OnCheckedChanged;

    public string Text { get; set; } = "";
    public bool Checked { get; private set; }
    public int BoxSize { get; set; } = 20;

    public CheckBox()
    {
        Focusable = true;
        PreferredSize = new System.Numerics.Vector2(160, 32);
    }

    public CheckBox(string text, bool isChecked = false)
        : this()
    {
        Text = text;
        Checked = isChecked;
    }

    public void SetChecked(bool value)
    {
        if (Checked == value)
            return;

        Checked = value;
        OnCheckedChanged?.Invoke(this, new GuiValueChangedEventArgs<bool>(Checked));
    }

    protected override void HandleClick(GuiMouseEventArgs args)
    {
        if (args.Button == MouseButton.Left)
            SetChecked(!Checked);
    }

    protected override void HandleKeyDown(GuiKeyEventArgs args)
    {
        if (args.Key is KeyboardKey.Enter or KeyboardKey.Space)
            SetChecked(!Checked);
    }

    public override void Draw()
    {
        Rectangle box = new(BoundingBox.X, BoundingBox.Y + (BoundingBox.Height - BoxSize) / 2, BoxSize, BoxSize);
        Color fill = Enabled ? Theme.Surface : Theme.Disabled;

        DrawBox(box, fill, IsFocused ? Theme.Focus : Theme.Border);

        if (Checked)
        {
            int pad = Math.Max(4, BoxSize / 5);
            Raylib.DrawRectangle(
                (int)box.X + pad,
                (int)box.Y + pad,
                BoxSize - pad * 2,
                BoxSize - pad * 2,
                Theme.Accent);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            int fontSize = Theme.FontSize;
            int x = (int)(box.X + BoxSize + Theme.Padding);
            int y = (int)(BoundingBox.Y + (BoundingBox.Height - fontSize) / 2);
            Raylib.DrawText(Text, x, y, fontSize, Enabled ? ResolvedTextColor : Theme.MutedText);
        }
    }
}
