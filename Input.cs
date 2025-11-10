#pragma warning disable IDE0011, IDE0046, IDE0058
using System.Diagnostics.CodeAnalysis;

namespace TipeUtils
{
    public class Input : IDisposable
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
                return false;
            Tokens.Add(Formatting.Split(input));
            return true;
        }

        public string[] GetTokens()
        {
            return [.. Tokens];
        }

        public string? GetToken()
        {
            string? token;
            while ((token = Tokens.GetToken()) is null)
            {
                if (!PopulateTokens())
                    return null;
            }

            return token;
        }

        public T? Read<T>()
        {
            string? token = GetToken();
            if (token == null)
                return default;
            return Parser.FromString<T>(token);
        }

        public bool TryRead<T>([NotNullWhen(true)] out T? value)
        {
            value = Read<T>();
            return value is not null;
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
