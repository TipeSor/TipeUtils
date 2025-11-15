using System.Text;
#pragma warning disable IDE0011, IDE0022, IDE0046, IDE0058
namespace TipeUtils
{
    public static class Formatting
    {
        internal static string Stringify(object? obj)
        {
            return obj?.ToString() ?? string.Empty;
        }

        public static string LeftPad(object? obj, int length, char fill = ' ')
        {
            string text = Stringify(obj);
            return text.Length >= length
                ? text[..length]
                : text + new string(fill, length - text.Length);
        }

        public static string RightPad(object? obj, int length, char fill = ' ')
        {
            string text = Stringify(obj);
            return text.Length >= length
                ? text[..length]
                : new string(fill, length - text.Length) + text;
        }

        public static string CenterPad(object? obj, int length, char fill = ' ')
        {
            string text = Stringify(obj);
            if (text.Length >= length)
            {
                return text[..length];
            }

            int totalPad = length - text.Length;
            int leftPad = totalPad / 2;
            int rightPad = totalPad - leftPad;

            return new string(fill, leftPad) + text + new string(fill, rightPad);
        }

        public static IEnumerable<string> Split(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) yield break;

            StringBuilder current = new();

            bool escape = false;
            bool inQuotes = false;

            foreach (char c in input)
            {
                if (escape) { current.Append(c); escape = false; continue; }
                if (c == '\\') { escape = true; continue; }
                if (c == '"') { inQuotes = !inQuotes; continue; }

                if (!inQuotes && (char.IsSeparator(c) || char.IsWhiteSpace(c)))
                {
                    string temp = current.ToString();
                    if (!string.IsNullOrWhiteSpace(temp)) yield return temp;
                    current.Clear();
                    continue;
                }
                current.Append(c);
            }

            if (current.Length > 0)
                yield return current.ToString();
        }
    }
}
