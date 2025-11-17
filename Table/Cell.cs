namespace TipeUtils.Tables
{
    public class Cell(string value = "", CellConfig? config = null)
    {
        public string Value { get; set; } = value;
        public CellConfig Config { get; set; } = config ?? new CellConfig();

        public static readonly Cell Empty = new("", CellConfig.Default);
    }
    public class CellConfig(
        CellPadding padding = CellPadding.Right,
        CellAlignment alignment = CellAlignment.Top,
        CellBorder border = CellBorder.None)
    {
        public CellPadding Padding { get; set; } = padding;
        public CellAlignment Alignment { get; set; } = alignment;
        public CellBorder Border { get; set; } = border;

        public static readonly CellConfig Default = new();
    }

    public enum CellPadding
    {
        Left,
        Center,
        Right
    }

    public enum CellAlignment
    {
        Top,
        Middle,
        Bottom
    }

    [Flags]
    public enum CellBorder
    {
        None = 0,
        Top = 1 << 0,
        Right = 1 << 1,
        Bottom = 1 << 2,
        Left = 1 << 3,
        All = Top | Right | Bottom | Left
    }
}
