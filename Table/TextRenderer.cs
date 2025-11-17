using TipeUtils.Formatting;

namespace TipeUtils.Tables
{
    public class TextRenderer(TextWriter writer, BorderChars? border = null) : ITableRenderer
    {
        private readonly TextWriter _writer = writer;
        private readonly BorderChars _border = border ?? BorderStyle.Box;

        public void Render(Table table)
        {
            uint rowCount = table.RowCount;
            uint colCount = table.ColCount;

            ColConfig[] colConfigs = new ColConfig[colCount + 1];
            for (uint c = 0; c <= colCount; c++)
                table.TryGetColConfig(c, out colConfigs[c]);

            RowConfig[] rowConfigs = new RowConfig[rowCount + 1];
            for (uint r = 0; r <= rowCount; r++)
                table.TryGetRowConfig(r, out rowConfigs[r]);

            char[,] charmap = new char[4, 4] {
                { '*'             , _border.Horizontal  , _border.Horizontal   , _border.Horizontal     },
                { _border.Vertical, _border.TopLeft     , _border.TopRight     , _border.TopJunction    },
                { _border.Vertical, _border.BottomLeft  , _border.BottomRight  , _border.BottomJunction },
                { _border.Vertical, _border.LeftJunction, _border.RightJunction, _border.Cross          }
            };

            for (uint row = 0; row <= rowCount; row++)
            {
                // Row separators
                RowConfig rowConfig = rowConfigs[row];
                for (uint col = 0; col <= colCount; col++)
                {
                    ColConfig colConfig = colConfigs[col];

                    // bottom-right
                    table.TryGet(row, col, out Cell cell1);

                    // bottom-left
                    Cell cell2 = new();
                    if (col > 0)
                        table.TryGet(row, col - 1, out cell2);

                    // top-right
                    Cell cell3 = new();
                    if (row > 0)
                        table.TryGet(row - 1, col, out cell3);

                    // top-left
                    Cell cell4 = new();
                    if (row > 0 && col > 0)
                        table.TryGet(row - 1, col - 1, out cell4);

                    CellBorder border1 = cell1.Config.Border;
                    CellBorder border2 = cell2.Config.Border;
                    CellBorder border3 = cell3.Config.Border;
                    CellBorder border4 = cell4.Config.Border;

                    bool leftFlag = border2.HasFlag(CellBorder.Top) ||
                                border4.HasFlag(CellBorder.Bottom);

                    bool rightFlag = border1.HasFlag(CellBorder.Top) ||
                                 border3.HasFlag(CellBorder.Bottom);

                    bool topFlag = border3.HasFlag(CellBorder.Left) ||
                               border4.HasFlag(CellBorder.Right);

                    bool bottomFlag = border1.HasFlag(CellBorder.Left) ||
                                  border2.HasFlag(CellBorder.Right);

                    int right = rightFlag ? 1 << 0 : 0;
                    int left = leftFlag ? 1 << 1 : 0;
                    int bottom = bottomFlag ? 1 << 0 : 0;
                    int top = topFlag ? 1 << 1 : 0;

                    int horizontal = right | left;

                    int vertical = bottom | top;

                    _writer.Write(charmap[vertical, horizontal]);
                    if (col < colCount) _writer.Write(new string(charmap[0, right], colConfig.Width));


                }
                if (row < rowCount)
                {
                    // Cell Logic
                    for (int h = 0; h < rowConfig.Height; h++)
                    {
                        _writer.WriteLine();
                        for (uint col = 0; col <= colCount; col++)
                        {
                            ColConfig colConfig = colConfigs[col];

                            // bottom-right
                            table.TryGet(row, col, out Cell cell1);

                            // bottom-left
                            Cell cell2 = new();
                            if (col > 0)
                                table.TryGet(row, col - 1, out cell2);

                            CellBorder border1 = cell1.Config.Border;
                            CellBorder border2 = cell2.Config.Border;

                            bool bottom = border1.HasFlag(CellBorder.Left) || border2.HasFlag(CellBorder.Right);

                            int y = bottom ? 1 : 0;
                            _writer.Write(charmap[y, 0]);

                            string value = cell1.Value;
                            value = cell1.Config.Padding switch
                            {
                                CellPadding.Left => StringUtils.RightPad(value, colConfig.Width),
                                CellPadding.Center => StringUtils.CenterPad(value, colConfig.Width),
                                CellPadding.Right => StringUtils.LeftPad(value, colConfig.Width),
                                _ => StringUtils.LeftPad(value, colConfig.Width),
                            };

                            int position = cell1.Config.Alignment switch
                            {
                                CellAlignment.Top => 0,
                                CellAlignment.Middle => (rowConfig.Height - 1) / 2,
                                CellAlignment.Bottom => rowConfig.Height - 1,
                                _ => 0,
                            };

                            if (col < colCount)
                            {
                                if (h == position) _writer.Write(value);
                                else _writer.Write(new string(' ', colConfig.Width));
                            }
                        }
                    }
                }

                // new line
                _writer.WriteLine();
            }
        }
    }

    public record BorderStyle
    {
        public static readonly BorderChars Ascii = new('-', '|', '+', '+', '+', '+', '+', '+', '+', '+', '+');
        public static readonly BorderChars Box = new('─', '│', '┌', '┐', '└', '┘', '┬', '┴', '├', '┤', '┼');
        public static readonly BorderChars DoubleBox = new('═', '║', '╔', '╗', '╚', '╝', '╦', '╩', '╠', '╣', '╬');
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
}
