using System.Text;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public static partial class StringBuilderANSI
{
    extension(ANSI)
    {
        public static StringBuilder Reset(StringBuilder sb)
            => sb.Append(ANSI.Reset);

        public static StringBuilder Background(StringBuilder sb, uint color)
            => Background(sb, (byte)(color >> 16), (byte)(color >> 8), (byte)color);

        public static StringBuilder Foreground(StringBuilder sb, uint color)
            => Foreground(sb, (byte)(color >> 16), (byte)(color >> 8), (byte)color);

        public static StringBuilder Background(StringBuilder sb, byte r, byte g, byte b)
            => sb.Append(ANSI.CSI).Append("48;2;").Append(r).Append(";").Append(g).Append(";").Append(b).Append("m");
        
        public static StringBuilder Foreground(StringBuilder sb, byte r, byte g, byte b)
            => sb.Append(ANSI.CSI).Append("38;2;").Append(r).Append(";").Append(g).Append(";").Append(b).Append("m");

        public static StringBuilder CUP(StringBuilder sb, int row, int column)
            => sb.Append(ANSI.CSI).Append(row + 1).Append(";").Append(column + 1).Append("H");
    }
}
