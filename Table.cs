using System.Text;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public class Table(uint rowCount, uint colCount, BorderChars? border = null)
    {
        private readonly Dictionary<(uint, uint), Cell> _cells = [];
        private readonly Dictionary<uint, RowConfig> _rowConfigs = [];
        private readonly Dictionary<uint, ColConfig> _colConfigs = [];

        private uint _rowCount = rowCount;
        private uint _colCount = colCount;

        private readonly BorderChars _border = border ?? new BorderChars(
            '─', '│', '┌', '┐', '└', '┘', '┬', '┴', '├', '┤', '┼'
        );

        public RowConfig GetRowConfig(uint row)
        {
            if (!_rowConfigs.TryGetValue(row, out RowConfig? config))
            {
                config = new RowConfig();
                _rowConfigs[row] = config;
            }

            return config;
        }

        public ColConfig GetColConfig(uint col)
        {
            if (!_colConfigs.TryGetValue(col, out ColConfig? config))
            {
                config = new ColConfig();
                _colConfigs[col] = config;
            }

            return config;
        }

        public Cell Get(uint row, uint col)
        {
            if (row + 1 > _rowCount) _rowCount = row + 1;
            if (col + 1 > _colCount) _colCount = col + 1;

            if (!_cells.TryGetValue((row, col), out Cell? cell))
            {
                cell = new Cell();
                _cells[(row, col)] = cell;
            }

            return cell;
        }

        public bool TryGetRowConfig(uint row, out RowConfig config)
        {
            bool res = _rowConfigs.TryGetValue(row, out RowConfig? value);
            config = value ?? new RowConfig();
            return res;
        }

        public bool TryGetColConfig(uint col, out ColConfig config)
        {
            bool res = _colConfigs.TryGetValue(col, out ColConfig? value);
            config = value ?? new ColConfig();
            return res;
        }

        public bool TryGet(uint row, uint col, out Cell cell)
        {
            bool res = _cells.TryGetValue((row, col), out Cell? value);
            cell = value ?? new Cell();
            return res;
        }

        public void OutlineRegion(uint startRow, uint startCol, uint endRow, uint endCol, OutlineMode mode = OutlineMode.Override)
        {
            for (uint row = startRow; row <= endRow; row++)
            {
                for (uint col = startCol; col <= endCol; col++)
                {
                    CellBorder border = CellBorder.None;
                    if (row == startRow) border |= CellBorder.Top;
                    if (col == startCol) border |= CellBorder.Left;
                    if (row == endRow) border |= CellBorder.Bottom;
                    if (col == endCol) border |= CellBorder.Right;

                    switch (mode)
                    {
                        case OutlineMode.Overlap:
                            this[row, col].Config.Border |= border;
                            break;
                        case OutlineMode.Override:
                            this[row, col].Config.Border = border;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void OutlineRow(uint row, OutlineMode mode = OutlineMode.Override)
        {
            for (uint col = 0; col < _colCount; col++)
            {
                CellBorder border = CellBorder.Top | CellBorder.Bottom;
                if (col == 0) border |= CellBorder.Left;
                if (col == _colCount - 1) border |= CellBorder.Right;

                switch (mode)
                {
                    case OutlineMode.Overlap:
                        this[row, col].Config.Border |= border;
                        break;
                    case OutlineMode.Override:
                        this[row, col].Config.Border = border;
                        break;
                    default:
                        break;
                }
            }
        }

        public void OutlineCol(uint col, OutlineMode mode = OutlineMode.Override)
        {
            for (uint row = 0; row < _rowCount; row++)
            {
                CellBorder border = CellBorder.Left | CellBorder.Right;
                if (row == 0) border |= CellBorder.Top;
                if (row == _rowCount - 1) border |= CellBorder.Bottom;

                switch (mode)
                {
                    case OutlineMode.Overlap:
                        this[row, col].Config.Border |= border;
                        break;
                    case OutlineMode.Override:
                        this[row, col].Config.Border = border;
                        break;
                    default:
                        break;
                }
            }
        }

        public Cell this[uint row, uint col] => Get(row, col);

        public override string ToString()
        {
            StringBuilder sb = new();
            char[,] charmap = new char[4, 4] {
                { ' '             , _border.Horizontal  , _border.Horizontal   , _border.Horizontal     },
                { _border.Vertical, _border.TopLeft     , _border.TopRight     , _border.TopJunction    },
                { _border.Vertical, _border.BottomLeft  , _border.BottomRight  , _border.BottomJunction },
                { _border.Vertical, _border.LeftJunction, _border.RightJunction, _border.Cross          }
            };

            for (uint row = 0; row <= _rowCount; row++)
            {
                // Row separators
                TryGetRowConfig(row, out RowConfig rowConfig);
                for (uint col = 0; col <= _colCount; col++)
                {
                    TryGetColConfig(col, out ColConfig? colConfig);

                    // bottom-right
                    Cell cell1;
                    TryGet(row - 0, col - 0, out cell1);

                    // bottom-left
                    Cell cell2 = new();
                    if (col > 0)
                        TryGet(row - 0, col - 1, out cell2);

                    // top-right
                    Cell cell3 = new();
                    if (row > 0)
                        TryGet(row - 1, col - 0, out cell3);

                    // top-left
                    Cell cell4 = new();
                    if (row > 0 && col > 0)
                        TryGet(row - 1, col - 1, out cell4);

                    CellBorder border1 = cell1.Config.Border;
                    CellBorder border2 = cell2.Config.Border;
                    CellBorder border3 = cell3.Config.Border;
                    CellBorder border4 = cell4.Config.Border;

                    bool left = border2.HasFlag(CellBorder.Top) ||
                                border4.HasFlag(CellBorder.Bottom);

                    bool right = border1.HasFlag(CellBorder.Top) ||
                                 border3.HasFlag(CellBorder.Bottom);

                    bool top = border3.HasFlag(CellBorder.Left) ||
                               border4.HasFlag(CellBorder.Right);

                    bool bottom = border1.HasFlag(CellBorder.Left) ||
                                  border2.HasFlag(CellBorder.Right);

                    int x = (left ? 2 : 0) + (right ? 1 : 0);

                    int y = (top ? 2 : 0) + (bottom ? 1 : 0);

                    sb.Append(charmap[y, x]);
                    if (col < _colCount) sb.Append(new string(charmap[0, right ? 1 : 0], colConfig.Width));


                }
                if (row < _rowCount)
                {
                    // Cell Logic
                    for (int h = 0; h < rowConfig.Height; h++)
                    {
                        sb.AppendLine();
                        for (uint col = 0; col <= _colCount; col++)
                        {
                            TryGetColConfig(col, out ColConfig? colConfig);

                            TryGet(row - 0, col - 0, out Cell cell1); // bottom-right
                            TryGet(row - 0, col - 1, out Cell cell2); // bottom-left

                            CellBorder border1 = cell1.Config.Border;
                            CellBorder border2 = cell2.Config.Border;

                            bool bottom = border1.HasFlag(CellBorder.Left) || border2.HasFlag(CellBorder.Right);

                            int y = bottom ? 1 : 0;
                            sb.Append(charmap[y, 0]);

                            string value = cell1.Value;
                            value = cell1.Config.Padding switch
                            {
                                CellPadding.Left => Formatting.LeftPad(value, colConfig.Width),
                                CellPadding.Center => Formatting.CenterPad(value, colConfig.Width),
                                CellPadding.Right => Formatting.RightPad(value, colConfig.Width),
                                _ => Formatting.LeftPad(value, colConfig.Width),
                            };

                            int position = cell1.Config.Alignment switch
                            {
                                CellAlignment.Top => 0,
                                CellAlignment.Middle => (rowConfig.Height - 1) / 2,
                                CellAlignment.Bottom => rowConfig.Height - 1,
                                _ => 0,
                            };

                            if (col < _colCount)
                            {
                                if (h == position) sb.Append(value);
                                else sb.Append(new string(' ', colConfig.Width));
                            }
                        }
                    }

                    // new line
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }

    public record BorderChars(
        char Horizontal,
        char Vertical,
        char TopLeft,
        char TopRight,
        char BottomLeft,
        char BottomRight,
        char TopJunction,
        char BottomJunction,
        char LeftJunction,
        char RightJunction,
        char Cross
    );

    public class RowConfig
    {
        public int Height { get; set; } = 1;
    }

    public class ColConfig
    {
        public int Width { get; set; } = 16;
    }

    public class CellConfig(
        CellPadding padding = CellPadding.Right,
        CellAlignment alignment = CellAlignment.Top,
        CellBorder border = CellBorder.None)
    {
        public CellPadding Padding { get; set; } = padding;
        public CellAlignment Alignment { get; set; } = alignment;
        public CellBorder Border { get; set; } = border;
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

    public class Cell(string value = "", CellConfig? config = null)
    {
        public string Value { get; set; } = value;
        public CellConfig Config { get; set; } = config ?? new CellConfig();
    }

    public enum OutlineMode
    {
        Overlap,
        Override
    }
}

