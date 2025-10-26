namespace TipeUtils
{
    public static class Formatting
    {
        public static string LeftPad(string? text, int length, char fill = ' ')
        {
            text ??= string.Empty;
            return text.Length >= length
                ? text[..length]
                : new string(fill, length - text.Length) + text;
        }

        public static string RightPad(string? text, int length, char fill = ' ')
        {
            text ??= string.Empty;
            return text.Length >= length
                ? text[..length]
                : text + new string(fill, length - text.Length);
        }

        public static string CenterPad(string? text, int length, char fill = ' ')
        {
            text ??= string.Empty;
            if (text.Length >= length)
            {
                return text[..length];
            }

            int totalPad = length - text.Length;
            int leftPad = totalPad / 2;
            int rightPad = totalPad - leftPad;

            return new string(fill, leftPad) + text + new string(fill, rightPad);
        }
    }
}
