using System.Text;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public class Table(int rowCount, int colCount, BorderChars? border = null)
    {
        private readonly Dictionary<(int, int), Cell> _cells = [];
        private readonly Dictionary<int, RowConfig> _rowConfigs = [];
        private readonly Dictionary<int, ColConfig> _colConfigs = [];

        private int _rowCount = rowCount;
        private int _colCount = colCount;

        private readonly BorderChars _border = border ?? new BorderChars(
            '─', '│', '┌', '┐', '└', '┘', '┬', '┴', '├', '┤', '┼'
        );

        public RowConfig GetRowConfig(int row)
        {
            if (!_rowConfigs.TryGetValue(row, out RowConfig? config))
            {
                config = new RowConfig();
                _rowConfigs[row] = config;
            }

            return config;
        }

        public ColConfig GetColConfig(int col)
        {
            if (!_colConfigs.TryGetValue(col, out ColConfig? config))
            {
                config = new ColConfig();
                _colConfigs[col] = config;
            }

            return config;
        }

        public Cell Get(int row, int col)
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

        public Cell this[int row, int col] => Get(row, col);

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine(Separator(_border.TopLeft, _border.TopJunction, _border.TopRight));

            for (int row = 0; row < _rowCount; row++)
            {
                sb.Append(_border.Vertical);

                for (int col = 0; col < _colCount; col++)
                {
                    ColConfig colConfig = GetColConfig(col);
                    Cell cell = Get(row, col);
                    string value = cell.Value;
                    CellConfig config = cell.Config;

                    value = config.Padding switch
                    {
                        CellPadding.LeftPad => Formatting.LeftPad(value, colConfig.Width),
                        CellPadding.RightPad => Formatting.RightPad(value, colConfig.Width),
                        CellPadding.CenterPad => Formatting.CenterPad(value, colConfig.Width),
                        _ => value
                    };

                    sb.Append(value);
                    sb.Append(_border.Vertical);
                }

                sb.AppendLine();

                if (row < _rowCount - 1)
                    sb.AppendLine(Separator(_border.LeftJunction, _border.Cross, _border.RightJunction));
                else
                    sb.AppendLine(Separator(_border.BottomLeft, _border.BottomJunction, _border.BottomRight));
            }

            return sb.ToString();
        }

        private string Separator(char left, char separator, char right)
        {
            List<string> segments = new(_colCount);

            for (int col = 0; col < _colCount; col++)
            {
                ColConfig colConfig = GetColConfig(col);
                segments.Add(new string(_border.Horizontal, colConfig.Width));
            }

            return left + string.Join(separator, segments) + right;
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

    public class CellConfig(CellPadding padding = CellPadding.RightPad)
    {
        public CellPadding Padding { get; set; } = padding;
    }

    public enum CellPadding
    {
        LeftPad,
        CenterPad,
        RightPad
    }

    public class Cell(string value = "", CellConfig? config = null)
    {
        public string Value { get; set; } = value;
        public CellConfig Config { get; set; } = config ?? new CellConfig();
    }
}

