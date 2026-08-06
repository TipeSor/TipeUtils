using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public class Panel : Container
{
    public GuiTheme Theme { get; set; } = GuiTheme.Default;
    public Color? BackgroundColor { get; set; }
    public Color? BorderColor { get; set; }
    public Thickness Padding { get; set; }
    public float Spacing { get; set; } = 4;
    public LayoutOrientation Orientation { get; set; } = LayoutOrientation.Vertical;

    public override void Draw()
    {
        DrawPanel();
        base.Draw();
    }

    protected override void LayoutChildren()
    {
        LayoutLinearChildren(ContentBounds(Padding), Orientation, Spacing);
    }

    private void DrawPanel()
    {
        Color fill = BackgroundColor ?? Theme.Surface;
        Color border = BorderColor ?? Theme.Border;
        float radius = Theme.CornerRadius;

        if (radius > 0)
        {
            float roundness = Math.Clamp(radius / MathF.Min(BoundingBox.Width, BoundingBox.Height), 0, 1);
            Raylib.DrawRectangleRounded(BoundingBox, roundness, Theme.CornerSegments, fill);
            Raylib.DrawRectangleRoundedLinesEx(BoundingBox, roundness, Theme.CornerSegments, Theme.BorderThickness, border);
            return;
        }

        Raylib.DrawRectangleRec(BoundingBox, fill);

        if (Theme.BorderThickness > 0)
            Raylib.DrawRectangleLinesEx(BoundingBox, Theme.BorderThickness, border);
    }
}
