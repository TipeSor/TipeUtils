#pragma warning disable IDE0011, IDE0022, IDE0046, IDE0058
using System.Text;
namespace TipeUtils
{
    public static class Tokenizer
    {
        public static IEnumerable<string> Tokenize(string input)
        {
            if (input == string.Empty) return [];

            List<string> tokens = [];
            StringBuilder current = new();
            Stack<char> stack = new();

            bool escape = false;
            bool inQuotes = false;

            Dictionary<char, char> pairs = new()
            {
                ['('] = ')',
                ['['] = ']',
                ['{'] = '}',
            };

            foreach (char c in input)
            {
                if (escape) { current.Append(c); escape = false; continue; }
                if (c == '\\') { escape = true; continue; }
                if (c == '"') { inQuotes = !inQuotes; continue; }

                if (stack.Count == 0 && !inQuotes && char.IsSeparator(c))
                {
                    string temp = current.ToString();
                    if (!string.IsNullOrWhiteSpace(temp)) tokens.Add(temp);
                    current.Clear();
                    continue;
                }

                if (!inQuotes)
                {
                    if (pairs.ContainsKey(c))
                        stack.Push(c);

                    if (pairs.ContainsValue(c))
                        if (stack.Count == 0 || c != pairs[stack.Pop()])
                            throw new InvalidOperationException($"Mismatched delimiter: '{c}'.");
                }
                current.Append(c);
            }

            if (escape)
                throw new InvalidOperationException("Unterminated escape sequence.");
            if (inQuotes)
                throw new InvalidOperationException("Unclosed quotes.");
            if (stack.Count > 0)
                throw new InvalidOperationException($"Unclosed delimiters, top delimiter: '{stack.Peek()}', total: ({stack.Count}).");

            if (current.Length > 0)
                tokens.Add(current.ToString());

            return [.. tokens];
        }

        public static bool TryTokenize(string input, out IEnumerable<string> tokens)
        {
            try { tokens = Tokenize(input); return true; }
            catch (Exception) { tokens = []; return false; }
        }
    }
}
