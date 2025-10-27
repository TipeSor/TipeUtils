#pragma warning disable IDE0011, IDE0022, IDE0046, IDE0058
namespace TipeUtils
{
    public class Output : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly bool _skipDispose;
        private bool _disposed;

        public Output()
        {
            _writer = Console.Out;
            _skipDispose = true;
        }

        public Output(string path, bool append = false, bool autoFlush = true)
        {
            _writer = new StreamWriter(path, append)
            {
                AutoFlush = autoFlush
            };
        }

        public Output(TextWriter writer, bool skipDispose = false)
        {
            _writer = writer;
            _skipDispose = skipDispose;
        }

        public void Write<T>(T obj) => _writer.Write(obj);
        public void Write(string format, params object[] args) => _writer.Write(format, args);

        public void WriteLine() => _writer.WriteLine();
        public void WriteLine<T>(T obj) => _writer.WriteLine(obj);

        public void Flush() => _writer.Flush();

        public void Close() => _writer.Close();

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
                _writer?.Dispose();
            }

            _disposed = true;
        }
    }
}
