#pragma warning disable IDE0011, IDE0022, IDE0046, IDE0058
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TipeUtils.IO
{
    public sealed class Output : TextWriter
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

        public override Encoding Encoding => Stream.Encoding;

        public override IFormatProvider FormatProvider => Stream.FormatProvider;

        [AllowNull]
        public override string NewLine
        {
            get => Stream.NewLine; set => Stream.NewLine = value;
        }

        public override void Flush() => Stream.Flush();
        public override Task FlushAsync() => Stream.FlushAsync();

        public override void Write(char value) => Stream.Write(value);
        public override void Write(char[]? buffer) => Stream.Write(buffer);
        public override void Write(char[] buffer, int index, int count) => Stream.Write(buffer, index, count);
        public override void Write(bool value) => Stream.Write(value);
        public override void Write(int value) => Stream.Write(value);
        public override void Write(uint value) => Stream.Write(value);
        public override void Write(long value) => Stream.Write(value);
        public override void Write(ulong value) => Stream.Write(value);
        public override void Write(float value) => Stream.Write(value);
        public override void Write(double value) => Stream.Write(value);
        public override void Write(decimal value) => Stream.Write(value);
        public override void Write(object? value) => Stream.Write(value);
        public override void Write(string? value) => Stream.Write(value);
        public override void Write(ReadOnlySpan<char> buffer) => Stream.Write(buffer);

        public override Task WriteAsync(char value) => Stream.WriteAsync(value);
        public override Task WriteAsync(char[] buffer, int index, int count) => Stream.WriteAsync(buffer, index, count);
        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
            => Stream.WriteAsync(buffer, cancellationToken);
        public override Task WriteAsync(string? value) => Stream.WriteAsync(value);

        public override void WriteLine() => Stream.WriteLine();
        public override void WriteLine(bool value) => Stream.WriteLine(value);
        public override void WriteLine(char value) => Stream.WriteLine(value);
        public override void WriteLine(char[]? buffer) => Stream.WriteLine(buffer);
        public override void WriteLine(char[] buffer, int index, int count) => Stream.WriteLine(buffer, index, count);
        public override void WriteLine(decimal value) => Stream.WriteLine(value);
        public override void WriteLine(double value) => Stream.WriteLine(value);
        public override void WriteLine(float value) => Stream.WriteLine(value);
        public override void WriteLine(int value) => Stream.WriteLine(value);
        public override void WriteLine(long value) => Stream.WriteLine(value);
        public override void WriteLine(object? value) => Stream.WriteLine(value);
        public override void WriteLine(string? value) => Stream.WriteLine(value);
        public override void WriteLine(uint value) => Stream.WriteLine(value);
        public override void WriteLine(ulong value) => Stream.WriteLine(value);
        public override void WriteLine(ReadOnlySpan<char> buffer) => Stream.WriteLine(buffer);

        public override Task WriteLineAsync() => Stream.WriteLineAsync();
        public override Task WriteLineAsync(char value) => Stream.WriteLineAsync(value);
        public override Task WriteLineAsync(char[] buffer, int index, int count) => Stream.WriteLineAsync(buffer, index, count);
        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
            => Stream.WriteLineAsync(buffer, cancellationToken);
        public override Task WriteLineAsync(string? value) => Stream.WriteLineAsync(value);

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (!_skipDispose)
                    Stream.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
