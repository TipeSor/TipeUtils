using System.Diagnostics.CodeAnalysis;
#pragma warning disable IDE0011, IDE0046, IDE0058
namespace TipeUtils
{
    public sealed class Input : IDisposable
    {
        public TextReader Stream { get; }
        private readonly bool _skipDispose;
        private bool _disposed;

        private LazyQueue<string> Tokens { get; set; } = [];

        public Input()
        {
            Stream = Console.In;
            _skipDispose = true;
        }

        public Input(string path)
        {
            Stream = new StreamReader(path);
        }

        public Input(TextReader reader, bool skipDispose = false)
        {
            Stream = reader;
            _skipDispose = skipDispose;
        }

        public string ReadLine()
        {
            return Stream.ReadLine() ?? string.Empty;
        }

        private bool PopulateTokens()
        {
            string input = ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            Tokens.Enqueue(Formatting.Split(input));
            return true;
        }

        public string[] GetTokens()
        {
            return [.. Tokens];
        }

        public string? GetToken()
        {
            string? token;
            while (!Tokens.TryDequeue(out token))
            {
                if (!PopulateTokens())
                    return null;
            }
            return token;
        }

        public T? Read<T>() where T : notnull
        {
            if (typeof(T).Implements(typeof(IInputable<>)))
            {
                object?[] args = [null];
                this.TryInvoke("TryReadComplex", [typeof(T)], out bool result, ref args);
                return result && args[0] is T item ? item : default;
            }
            return ReadSimple<T>();
        }

        public T? ReadSimple<T>() where T : notnull
        {
            string? token = GetToken();
            if (token == null)
                return default;

            if (Parser.TryFromString(token, out T? value))
                return value;

            return default;
        }

        public bool TryRead<T>([NotNullWhen(true)] out T? value) where T : notnull
        {
            value = Read<T>();
            return value is not null;
        }

        public bool TryReadSimple<T>([NotNullWhen(true)] out T? value) where T : notnull
        {

            value = ReadSimple<T>();
            return value is not null;
        }

        public bool TryReadComplex<T>([NotNullWhen(true)] out T? value) where T : notnull, IInputable<T>
        {
            return T.TryRead(this, out value);
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

        private void Dispose(bool disposing)
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

    public interface IInputable<T> where T : notnull, IInputable<T>
    {
        static abstract bool TryRead(Input input, [NotNullWhen(true)] out T? value);
    }
}
