#pragma warning disable IDE0011, IDE0046, IDE0058
using System.Diagnostics.CodeAnalysis;

namespace TipeUtils
{
    public class Input : IDisposable
    {
        public TextReader Stream { get; }
        private readonly Queue<string> _tokens = new();
        private readonly bool _skipDispose;
        private bool _disposed;

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
            if (!Tokenizer.TryTokenize(input, out IEnumerable<string>? tokens))
                return false;

            foreach (string token in tokens!)
                _tokens.Enqueue(token);

            return true;
        }

        public string[] GetCurrentTokens()
        {
            string[] tokens = [.. _tokens];
            _tokens.Clear();
            return tokens;
        }

        public string GetToken()
        {
            if (_tokens.Count == 0 && !PopulateTokens())
                throw new EndOfStreamException();
            return _tokens.Dequeue();
        }

        public string PeekToken()
        {
            if (_tokens.Count == 0 && !PopulateTokens())
                throw new EndOfStreamException();

            return _tokens.Peek();
        }

        public object? Read(Type type)
        {
            string token = GetToken();
            object? value = Parser.FromString(token, type);
            return value;
        }

        public bool TryRead(Type type, [NotNullWhen(true)] out object? value)
        {
            value = Read(type);
            return value is not null;
        }

        public T? Read<T>()
        {
            return (T?)Read(typeof(T));
        }

        public bool TryRead<T>([NotNullWhen(true)] out T? value)
        {
            bool res = TryRead(typeof(T), out object? raw);
            value = res ? (T)raw! : default;
            return res;
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
