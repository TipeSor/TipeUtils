namespace TipeUtils.Tables.Extensions
{
    public static class TableExtensions
    {
        public static void Configure(this Cell cell, object? value, CellPadding? padding, CellAlignment? alignment, CellBorder? border)
        {
            CellConfig config = cell.Config;
            cell.Value = value?.ToString() ?? cell.Value;
            config.Padding = padding ?? config.Padding;
            config.Alignment = alignment ?? config.Alignment;
            config.Border = border ?? config.Border;

        }

        public static void SetValue(this Cell cell, object? value)
        {
            cell.Value = value?.ToString() ?? "";
        }

        public static void SetPadding(this Cell cell, CellPadding padding)
        {
            cell.Configure(null, padding, null, null);
        }

        public static void SetAlignment(this Cell cell, CellAlignment alignment)
        {
            cell.Configure(null, null, alignment, null);
        }

        public static void SetBorder(this Cell cell, CellBorder border)
        {
            cell.Configure(null, null, null, border);
        }

        public static void SetRow(this Table table, uint row, params object?[] data)
        {
            for (uint col = 0; col < data.Length; col++)
            {
                table[row, col].SetValue(data[col]);
            }
        }

        public static void SetColumn(this Table table, uint col, params object?[] data)
        {
            for (uint row = 0; row < data.Length; row++)
            {
                table[row, col].SetValue(data[row]);
            }
        }

        public static void Outline(this Table table, uint startRow, uint startColumn, uint endRow, uint endColumn, bool replace = true)
        {
            for (uint col = startColumn; col <= endColumn; col++)
            {
                for (uint row = startRow; row <= endRow; row++)
                {
                    CellBorder border = CellBorder.None;

                    if (col == startColumn)
                        border |= CellBorder.Left;
                    if (col == endColumn)
                        border |= CellBorder.Right;
                    if (row == startRow)
                        border |= CellBorder.Top;
                    if (row == endRow)
                        border |= CellBorder.Bottom;

                    CellConfig config = table[row, col].Config;
                    if (replace)
                        config.Border = border;
                    else
                        config.Border |= border;
                }
            }
        }

        public static void OutlineRows(this Table table, uint startRow, uint endRow, bool replace = true)
        {
            table.Outline(startRow, 0, endRow, table.ColCount - 1, replace);
        }

        public static void OutlineCols(this Table table, uint startCol, uint endCol, bool replace = true)
        {
            table.Outline(0, startCol, table.RowCount - 1, endCol, replace);
        }

        public static void OutlineRow(this Table table, uint row, bool replace = true)
        {
            table.OutlineRows(row, row, replace);
        }

        public static void OutlineCol(this Table table, uint col, bool replace = true)
        {
            table.OutlineCols(col, col, replace);
        }
    }
}
