using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

#if !TipeUtilsNoNamespace
namespace TipeUtils;
#endif

public static partial class ConsoleEx
{
    public static string Format(
        object? value, 
        string? format = null, 
        IFormatProvider? formatProvider = null)
    {
        return value is IFormattable f ? f.ToString(format, formatProvider) : value?.ToString() ?? "null";
    }

    public static string FormatArray(
        IEnumerable arr,
        string? format = null,
        IFormatProvider? formatProvider = null)
    {
        return FormatArray(new(), arr, format, formatProvider).ToString();
    }
    
    public static StringBuilder FormatArray(
        StringBuilder sb,
        IEnumerable arr,
        string? format = null,
        IFormatProvider? formatProvider = null)
    {
        sb.Append("[");

        bool first = true;
        foreach (object item in arr)
        {
            if (first) first = false;
            else sb.Append(", ");

            if (item is IEnumerable arr2 and not string)
            {
                FormatArray(sb, arr2, format, formatProvider);
                continue;
            }

            sb.Append(Format(item, format, formatProvider));
        }

        sb.Append("]");
        return sb;
    }

    public static void Log(
        object value,
        string? format = null,
        IFormatProvider? formatProvider = null,
        [CallerArgumentExpression(nameof(value))] string? expr = null)
    {
        Console.Write($"{expr}: {Format(value, format, formatProvider)}");
    }

    public static void LogArray(
        IEnumerable arr,
        string? format = null,
        IFormatProvider? formatProvider = null,
        [CallerArgumentExpression(nameof(arr))] string? expr = null)
    {
        Console.Write(expr);
        Console.Write(": ");
        Console.Write(FormatArray(arr, format, formatProvider));
    }

    public static void LogProgress(int value, int maxValue, string? name = null)
    {
        if (name != null)
            Console.Write($"{name}: ");

        int width = maxValue.ToString().Length;

        Console.Write("[");
        Console.Write((value + 1).ToString($"D{width}"));
        Console.Write("/");
        Console.Write(maxValue);
        Console.Write("]");
    }
}
