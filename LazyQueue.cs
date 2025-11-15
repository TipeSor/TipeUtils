using System.Collections;
using System.Diagnostics.CodeAnalysis;
#pragma warning disable IDE0011, IDE0022, IDE0046, IDE0058
namespace TipeUtils
{
    public class LazyQueue<T> : IDisposable, IEnumerable<T> where T : notnull
    {
        private readonly Queue<IEnumerable<T>> _sources = new();
        private IEnumerator<T>? _currentEnumerator;
        private bool _disposed;

        private T? _peekedItem;
        private bool _peeked;

        private readonly List<T> _tinyItems = [];

        public LazyQueue() { }

        public LazyQueue(IEnumerable<T> source)
        {
            _sources.Enqueue(source);
        }

        public void Enqueue(IEnumerable<T> source)
        {
            FlushTinyItems();
            _sources.Enqueue(source);
        }

        public void Enqueue(T item)
        {
            _tinyItems.Add(item);
        }

        private void FlushTinyItems()
        {
            if (_tinyItems.IsEmpty()) return;
            _sources.Enqueue([.. _tinyItems]);
            _tinyItems.Clear();
        }

        public T Peek()
        {
            if (_peeked) return _peekedItem!;
            _peekedItem = Dequeue();
            _peeked = true;
            return _peekedItem;
        }

        public T Dequeue()
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
                    if (_sources.IsEmpty())
                    {
                        FlushTinyItems();
                        if (_sources.IsEmpty())
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
