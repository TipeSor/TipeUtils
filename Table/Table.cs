namespace TipeUtils.Tables
{
    public class Table(uint rowCount, uint colCount)
    {
        private readonly Dictionary<(uint, uint), Cell> _cells = [];
        private readonly Dictionary<uint, RowConfig> _rowConfigs = [];
        private readonly Dictionary<uint, ColConfig> _colConfigs = [];

        public uint RowCount { get; private set; } = rowCount;
        public uint ColCount { get; private set; } = colCount;

        public Table() :
            this(0, 0)
        { }

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
            if (row + 1 > RowCount) RowCount = row + 1;
            if (col + 1 > ColCount) ColCount = col + 1;

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
            config = value ?? RowConfig.Default;
            return res;
        }

        public bool TryGetColConfig(uint col, out ColConfig config)
        {
            bool res = _colConfigs.TryGetValue(col, out ColConfig? value);
            config = value ?? ColConfig.Default;
            return res;
        }

        public bool TryGet(uint row, uint col, out Cell cell)
        {
            bool res = _cells.TryGetValue((row, col), out Cell? value);
            cell = value ?? Cell.Empty;
            return res;
        }

        public Cell this[uint row, uint col] => Get(row, col);
    }



    public class RowConfig
    {
        public int Height { get; set; } = 1;

        public static readonly RowConfig Default = new();
    }

    public class ColConfig
    {
        public int Width { get; set; } = 16;

        public static readonly ColConfig Default = new();
    }


}
