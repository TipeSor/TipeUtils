namespace TipeUtils.Tables
{
    public static class TableExtensions
    {
        extension(Cell cell)
        {
            public void Configure(object? value, CellPadding? padding, CellAlignment? alignment, CellBorder? border)
            {
                CellConfig config = cell.Config;
                cell.Value = value?.ToString() ?? cell.Value;
                config.Padding = padding ?? config.Padding;
                config.Alignment = alignment ?? config.Alignment;
                config.Border = border ?? config.Border;
            }

            public void SetValue(object? value)
            {
                cell.Value = value?.ToString() ?? "";
            }

            public void SetPadding(CellPadding padding)
            {
                cell.Configure(null, padding, null, null);
            }

            public void SetAlignment(CellAlignment alignment)
            {
                cell.Configure(null, null, alignment, null);
            }

            public void SetBorder(CellBorder border)
            {
                cell.Configure(null, null, null, border);
            }
        }

        extension(Table table)
        {
            public void SetRow(uint row, params object?[] data)
            {
                for (uint col = 0; col < data.Length; col++)
                {
                    table[row, col].SetValue(data[col]);
                }
            }

            public void SetColumn(uint col, params object?[] data)
            {
                for (uint row = 0; row < data.Length; row++)
                {
                    table[row, col].SetValue(data[row]);
                }
            }

            public void Outline(uint startRow, uint startColumn, uint endRow, uint endColumn, bool replace = true)
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

            public void OutlineRows(uint startRow, uint endRow, bool replace = true)
            {
                table.Outline(startRow, 0, endRow, table.ColCount - 1, replace);
            }

            public void OutlineCols(uint startCol, uint endCol, bool replace = true)
            {
                table.Outline(0, startCol, table.RowCount - 1, endCol, replace);
            }

            public void OutlineRow(uint row, bool replace = true)
            {
                table.OutlineRows(row, row, replace);
            }

            public void OutlineCol(uint col, bool replace = true)
            {
                table.OutlineCols(col, col, replace);
            }
        }
    }
}
