namespace TipeUtils.Collections
{
    public class LazyQueue<T> : IDisposable
    {
        private readonly Queue<IEnumerable<T>> _sources = new();
        private IEnumerator<T>? _currentEnumerator;
        private bool _disposed;

        private readonly int _tinyBufferSize;
        private readonly T[] _tinyBuffer;
        private int _tinyIndex;

        private readonly object _syncLock = new();

        public LazyQueue(int capacity)
        {
            _tinyBufferSize = capacity;
            _tinyBuffer = new T[_tinyBufferSize];
        }

        public LazyQueue() : this(64) { }

        public void Enqueue(IEnumerable<T> source)
        {
            lock (_syncLock)
            {
                FlushTinyItems();
                _sources.Enqueue(source);
            }
        }

        public void Enqueue(T item)
        {
            lock (_syncLock)
            {
                if (_tinyIndex >= _tinyBufferSize)
                    FlushTinyItems();
                _tinyBuffer[_tinyIndex++] = item;
            }
        }

        private void FlushTinyItems()
        {
            if (_tinyIndex == 0) return;
            T[] buffer = new T[_tinyIndex];
            Array.Copy(_tinyBuffer, 0, buffer, 0, _tinyIndex);
            _sources.Enqueue(buffer);
            Array.Clear(_tinyBuffer, 0, _tinyIndex);
            _tinyIndex = 0;
        }

        private Result<T, string> DequeueUnsafe()
        {
            while (true)
            {
                if (_currentEnumerator == null)
                {
                    if (_sources.Count == 0)
                        FlushTinyItems();

                    if (_sources.Count == 0)
                        return Result<T, string>.Err("Queue is empty");

                    _currentEnumerator = _sources.Dequeue().GetEnumerator();
                }

                if (_currentEnumerator.MoveNext())
                    return Result<T, string>.Ok(_currentEnumerator.Current);

                _currentEnumerator.Dispose();
                _currentEnumerator = null;
            }
        }

        public Result<T, string> Dequeue()
        {
            lock (_syncLock)
            {
                return DequeueUnsafe();
            }
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

            if (disposing)
            {
                _currentEnumerator?.Dispose();
            }

            _disposed = true;
        }
    }
}
