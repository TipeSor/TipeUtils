#pragma warning disable IDE0011, IDE0046, IDE0058
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

        public Output(string path)
        {
            _writer = new StreamWriter(path);
        }

        public Output(TextWriter writer)
        {
            _writer = writer;
        }

        public void Write<T>(T obj)
        {
            _writer.Write(obj);
        }

        public void WriteLine<T>(T obj)
        {
            _writer.WriteLine(obj);
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Close()
        {
            _writer.Close();
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
                _writer?.Dispose();
            }

            _disposed = true;
        }
    }
}
