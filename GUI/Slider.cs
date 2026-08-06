using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public sealed class Slider : Control
{
    public event EventHandler<GuiValueChangedEventArgs<float>>? OnValueChanged;

    public float Minimum { get; set; }
    public float Maximum { get; set; } = 1;
    public float Value { get; private set; }
    public float Step { get; set; } = 0.1f;

    public Slider()
    {
        Focusable = true;
        PreferredSize = new System.Numerics.Vector2(180, 28);
    }

    public Slider(float minimum, float maximum, float value)
        : this()
    {
        Minimum = minimum;
        Maximum = maximum;
        SetValue(value);
    }

    public void SetValue(float value)
    {
        float min = MathF.Min(Minimum, Maximum);
        float max = MathF.Max(Minimum, Maximum);
        float next = Snap(Math.Clamp(value, min, max));

        if (Math.Abs(Value - next) < float.Epsilon)
            return;

        Value = next;
        OnValueChanged?.Invoke(this, new GuiValueChangedEventArgs<float>(Value));
    }

    protected override void HandleMouseDown(GuiMouseEventArgs args)
    {
        if (args.Button != MouseButton.Left)
            return;

        CaptureMouse();
        SetValueFromPosition(args.Position);
    }

    protected override void HandleMouseMove(GuiMouseEventArgs args)
    {
        if (HasMouseCapture)
            SetValueFromPosition(args.Position);
    }

    protected override void HandleMouseUp(GuiMouseEventArgs args)
    {
        if (args.Button == MouseButton.Left)
            ReleaseMouse();
    }

    protected override void HandleKeyDown(GuiKeyEventArgs args)
    {
        if (args.Key == KeyboardKey.Left)
            SetValue(Value - Step);

        if (args.Key == KeyboardKey.Right)
            SetValue(Value + Step);
    }

    public override void Draw()
    {
        float trackHeight = MathF.Max(4, BoundingBox.Height / 5);
        float knobSize = MathF.Min(BoundingBox.Height, 18);
        float trackY = BoundingBox.Y + (BoundingBox.Height - trackHeight) / 2;
        float knobX = ValueToX(Value) - knobSize / 2;

        Rectangle track = new(BoundingBox.X, trackY, BoundingBox.Width, trackHeight);
        Rectangle filled = new(BoundingBox.X, trackY, MathF.Max(0, ValueToX(Value) - BoundingBox.X), trackHeight);
        Rectangle knob = new(knobX, BoundingBox.Y + (BoundingBox.Height - knobSize) / 2, knobSize, knobSize);

        Raylib.DrawRectangleRec(track, Enabled ? Theme.SurfaceHover : Theme.Disabled);
        Raylib.DrawRectangleRec(filled, Theme.Accent);
        Raylib.DrawRectangleRec(knob, Enabled ? Theme.Surface : Theme.Disabled);
        Raylib.DrawRectangleLinesEx(knob, Theme.BorderThickness, IsFocused ? Theme.Focus : Theme.Border);
    }

    private void SetValueFromPosition(System.Numerics.Vector2 position)
    {
        float ratio = BoundingBox.Width <= 0 ? 0 : (position.X - BoundingBox.X) / BoundingBox.Width;
        float value = Minimum + Math.Clamp(ratio, 0, 1) * (Maximum - Minimum);
        SetValue(value);
    }

    private float ValueToX(float value)
    {
        float range = Maximum - Minimum;

        if (Math.Abs(range) < float.Epsilon)
            return BoundingBox.X;

        float ratio = (value - Minimum) / range;
        return BoundingBox.X + Math.Clamp(ratio, 0, 1) * BoundingBox.Width;
    }

    private float Snap(float value)
    {
        if (Step <= 0)
            return value;

        return Minimum + MathF.Round((value - Minimum) / Step) * Step;
    }
}
