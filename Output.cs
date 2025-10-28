#pragma warning disable IDE0011, IDE0022, IDE0046, IDE0058
namespace TipeUtils
{
    public class Output : IDisposable
    {
        public TextWriter Stream { get; }
        private readonly bool _skipDispose;
        private bool _disposed;

        public Output()
        {
            Stream = new StreamWriter(Console.OpenStandardOutput(), leaveOpen: true)
            {
                AutoFlush = true,
            };
            _skipDispose = true;
        }

        public Output(string path, bool append = false, bool autoFlush = true)
        {
            Stream = new StreamWriter(path, append)
            {
                AutoFlush = autoFlush
            };
        }

        public Output(TextWriter writer, bool skipDispose = false)
        {
            Stream = writer;
            _skipDispose = skipDispose;
        }

        public void Write<T>(T obj) => Stream.Write(obj);
        public void Write(string format, params object?[] args) => Stream.Write(format, args);

        public void WriteLine() => Stream.WriteLine();
        public void WriteLine<T>(T obj) => Stream.WriteLine(obj);

        public void Flush() => Stream.Flush();
        public void Close() => Stream.Close();

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
