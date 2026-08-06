using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public sealed class ToggleButton : Button
{
    public event EventHandler<GuiValueChangedEventArgs<bool>>? OnToggled;

    public bool IsToggled { get; private set; }
    public Color? ToggledColor { get; set; }

    public ToggleButton() { }

    public ToggleButton(string text, bool isToggled = false)
        : base(text)
    {
        IsToggled = isToggled;
    }

    public void SetToggled(bool value)
    {
        if (IsToggled == value)
            return;

        IsToggled = value;
        OnToggled?.Invoke(this, new GuiValueChangedEventArgs<bool>(IsToggled));
    }

    protected override void HandleClick(GuiMouseEventArgs args)
    {
        if (args.Button == MouseButton.Left)
            SetToggled(!IsToggled);
    }

    public override void Draw()
    {
        Color? originalBackground = BackgroundColor;

        if (IsToggled)
            BackgroundColor = ToggledColor ?? Theme.Accent;

        base.Draw();
        BackgroundColor = originalBackground;
    }
}
