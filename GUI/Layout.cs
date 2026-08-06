using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public enum LayoutOrientation
{
    Vertical,
    Horizontal
}

public readonly struct Thickness
{
    public Thickness(float uniform)
        : this(uniform, uniform, uniform, uniform) { }

    public Thickness(float horizontal, float vertical)
        : this(horizontal, vertical, horizontal, vertical) { }

    public Thickness(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public float Left { get; }
    public float Top { get; }
    public float Right { get; }
    public float Bottom { get; }

    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;
}

public abstract class Control : Widget
{
    public GuiTheme Theme { get; set; } = GuiTheme.Default;
    public Color? TextColor { get; set; }
    public Color? BackgroundColor { get; set; }
    public Color? HoverColor { get; set; }
    public Color? PressedColor { get; set; }
    public Color? BorderColor { get; set; }
    public int? FontSize { get; set; }
    public float? CornerRadius { get; set; }

    protected int ResolvedFontSize => FontSize ?? Theme.FontSize;

    protected Color ResolvedTextColor => TextColor ?? Theme.Text;

    protected Color ResolveSurfaceColor()
    {
        if (!Enabled)
            return Theme.Disabled;

        if (IsPressed)
            return PressedColor ?? Theme.SurfacePressed;

        if (IsHovered)
            return HoverColor ?? Theme.SurfaceHover;

        return BackgroundColor ?? Theme.Surface;
    }

    protected void DrawBackground()
    {
        DrawBox(BoundingBox, ResolveSurfaceColor(), BorderColor ?? Theme.Border);

        if (IsFocused)
            DrawBorder(BoundingBox, Theme.Focus, Theme.BorderThickness + 1);
    }

    protected void DrawBox(Rectangle rectangle, Color fill, Color border)
    {
        float radius = CornerRadius ?? Theme.CornerRadius;

        if (radius > 0)
        {
            float roundness = Math.Clamp(radius / MathF.Min(rectangle.Width, rectangle.Height), 0, 1);
            Raylib.DrawRectangleRounded(rectangle, roundness, Theme.CornerSegments, fill);
            Raylib.DrawRectangleRoundedLinesEx(rectangle, roundness, Theme.CornerSegments, Theme.BorderThickness, border);
            return;
        }

        Raylib.DrawRectangleRec(rectangle, fill);
        Raylib.DrawRectangleLinesEx(rectangle, Theme.BorderThickness, border);
    }

    protected void DrawBorder(Rectangle rectangle, Color color, float thickness)
    {
        float radius = CornerRadius ?? Theme.CornerRadius;

        if (radius > 0)
        {
            float roundness = Math.Clamp(radius / MathF.Min(rectangle.Width, rectangle.Height), 0, 1);
            Raylib.DrawRectangleRoundedLinesEx(rectangle, roundness, Theme.CornerSegments, thickness, color);
            return;
        }

        Raylib.DrawRectangleLinesEx(rectangle, thickness, color);
    }

    protected void DrawCenteredText(string text, Color color, int fontSize)
    {
        int width = Raylib.MeasureText(text, fontSize);
        int x = (int)(BoundingBox.X + (BoundingBox.Width - width) / 2);
        int y = (int)(BoundingBox.Y + (BoundingBox.Height - fontSize) / 2);
        Raylib.DrawText(text, x, y, fontSize, color);
    }
}

public class StackPanel : Container
{
    public LayoutOrientation Orientation { get; set; } = LayoutOrientation.Vertical;
    public Thickness Padding { get; set; }
    public float Spacing { get; set; } = 4;

    protected override void LayoutChildren()
    {
        LayoutLinearChildren(ContentBounds(Padding), Orientation, Spacing);
    }
}

public sealed class Column : StackPanel
{
    public Column()
    {
        Orientation = LayoutOrientation.Vertical;
    }
}

public sealed class Row : StackPanel
{
    public Row()
    {
        Orientation = LayoutOrientation.Horizontal;
    }
}

public sealed class Grid : Container
{
    public int Columns { get; set; } = 1;
    public int Rows { get; set; } = 1;
    public Thickness Padding { get; set; }
    public float Spacing { get; set; } = 4;

    protected override void LayoutChildren()
    {
        int columns = Math.Max(1, Columns);
        int rows = Math.Max(1, Rows);
        Rectangle content = ContentBounds(Padding);
        float cellWidth = (content.Width - Spacing * (columns - 1)) / columns;
        float cellHeight = (content.Height - Spacing * (rows - 1)) / rows;

        for (int i = 0; i < Children.Count; i++)
        {
            Widget child = Children[i];

            if (!child.Visible)
                continue;

            int column = i % columns;
            int row = i / columns;

            if (row >= rows)
                break;

            child.BoundingBox = new Rectangle(
                content.X + column * (cellWidth + Spacing),
                content.Y + row * (cellHeight + Spacing),
                cellWidth,
                cellHeight);
        }
    }
}
