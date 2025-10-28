using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public class Input : IDisposable
    {
        public TextReader Stream { get; }
        private readonly bool _skipDispose;
        private int _currentChar;
        private bool _disposed;

        public Input()
        {
            Stream = new StreamReader(Console.OpenStandardInput(), leaveOpen: true);
            _skipDispose = true;
        }

        public Input(string path)
        {
            Stream = new StreamReader(path);
        }

        public Input(TextReader reader, bool skipDispose)
        {
            Stream = reader;
            _skipDispose = skipDispose;
        }

        private void NextChar()
        {
            _currentChar = Stream.Read();
        }

        private bool IsSeparator()
        {
            return char.IsWhiteSpace((char)_currentChar);
        }

        private string GetToken()
        {
            StringBuilder sb = new();

            while (true)
            {
                NextChar();
                if (_currentChar == -1 || !IsSeparator())
                    break;
            }

            while (_currentChar != -1 && !IsSeparator())
            {
                sb.Append((char)_currentChar);
                NextChar();
            }

            if (sb.Length == 0 && _currentChar == -1)
                throw new EndOfStreamException("Unexpected end of input.");

            return sb.ToString();
        }

        public T Read<T>()
        {
            string token = GetToken();
            Type targetType = typeof(T);

            if (Nullable.GetUnderlyingType(targetType) is Type underlyingType)
            {
                targetType = underlyingType;
                if (string.IsNullOrWhiteSpace(token))
                    return default!;
            }

            if (targetType.IsEnum)
                return (T)Enum.Parse(targetType, token, ignoreCase: true);

            if (typeof(IConvertible).IsAssignableFrom(targetType))
                return (T)Convert.ChangeType(token, targetType, CultureInfo.InvariantCulture);

            TypeConverter converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
                return (T)converter.ConvertFromString(token)!;

            MethodInfo? parseMethod = targetType.GetMethod("Parse", [typeof(string)]);
            if (parseMethod != null)
                return (T)parseMethod.Invoke(null, [token])!;

            throw new InvalidOperationException($"Failed to parse token '{token}' as {typeof(T).Name}.");
        }

        public void Close()
        {
            Stream.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && !_skipDispose)
            {
                Stream?.Dispose();
            }

            _disposed = true;
        }
    }
}
