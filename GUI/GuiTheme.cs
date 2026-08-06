using Raylib_cs;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public class GuiTheme
{
    public Color Text { get; set; } = Color.Black;
    public Color MutedText { get; set; } = Color.DarkGray;
    public Color Background { get; set; } = Color.RayWhite;
    public Color Surface { get; set; } = Color.White;
    public Color SurfaceHover { get; set; } = Color.LightGray;
    public Color SurfacePressed { get; set; } = Color.Gray;
    public Color Border { get; set; } = Color.Gray;
    public Color Accent { get; set; } = Color.SkyBlue;
    public Color Disabled { get; set; } = Color.LightGray;
    public Color Focus { get; set; } = Color.Blue;

    public int FontSize { get; set; } = 20;
    public int Padding { get; set; } = 8;
    public float BorderThickness { get; set; } = 1;
    public float CornerRadius { get; set; } = 0;
    public int CornerSegments { get; set; } = 8;

    public static GuiTheme Default { get; } = new();
    public static GuiTheme Rounded { get; } = new() { CornerRadius = 6 };
    public static GuiTheme Compact { get; } = new() { FontSize = 16, Padding = 6 };
}
