using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace TipeUtils.Collections
{
    public class LazyQueue<T> : IDisposable, IEnumerable<T> where T : notnull
    {
        private readonly Queue<IEnumerable<T>> _sources = new();
        private IEnumerator<T>? _currentEnumerator;
        private bool _disposed;

        private T? _peekedItem;
        private bool _peeked;

        private readonly List<T> _tinyItems = [];
        private readonly object _syncLock = new();

        public LazyQueue() { }

        public LazyQueue(IEnumerable<T> source)
        {
            _sources.Enqueue(source);
        }

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
                _tinyItems.Add(item);
        }

        private void FlushTinyItems()
        {
            if (_tinyItems.Count == 0) return;
            _sources.Enqueue([.. _tinyItems]);
            _tinyItems.Clear();
        }

        public T Peek()
        {
            lock (_syncLock)
            {
                if (_peeked) return _peekedItem!;
                _peekedItem = DequeueUnsafe();
                _peeked = true;
                return _peekedItem;
            }
        }

        private T DequeueUnsafe()
        {
            while (true)
            {
                if (_peeked)
                {
                    _peeked = false;
                    return _peekedItem!;
                }

                if (_currentEnumerator == null)
                {
                    if (_sources.Count == 0)
                    {
                        FlushTinyItems();
                        if (_sources.Count == 0)
                            throw new InvalidOperationException("LazyQueue is empty");
                    }
                    _currentEnumerator = _sources.Dequeue().GetEnumerator();
                }

                if (_currentEnumerator.MoveNext())
                    return _currentEnumerator.Current;

                _currentEnumerator.Dispose();
                _currentEnumerator = null;
            }
        }

        public T Dequeue()
        {
            lock (_syncLock)
            {
                return DequeueUnsafe();
            }
        }

        public bool TryDequeue([NotNullWhen(true)] out T? value)
        {
            try { value = Dequeue(); return true; }
            catch (InvalidOperationException) { value = default; return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (TryDequeue(out T? item))
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
