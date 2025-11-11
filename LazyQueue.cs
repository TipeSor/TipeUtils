using System.Collections;
#pragma warning disable IDE0011, IDE0022, IDE0046, IDE0058
namespace TipeUtils
{
    public class LazyQueue<T> : IEnumerable<T>
    {
        private readonly Queue<IEnumerable<T>> _sources = new();
        private IEnumerator<T>? _currentEnumerator;

        public LazyQueue() { }

        public LazyQueue(IEnumerable<T> source)
        {
            _sources.Enqueue(source);
        }

        public void Enqueue(IEnumerable<T> source)
        {
            _sources.Enqueue(source);
        }

        public void Enqueue(T item)
        {
            _sources.Enqueue([item]);
        }

        public T? Dequeue()
        {
            while (true)
            {
                if (_currentEnumerator == null)
                {
                    if (_sources.Count == 0)
                        return default;
                    _currentEnumerator = _sources.Dequeue().GetEnumerator();
                }

                if (_currentEnumerator!.MoveNext())
                    return _currentEnumerator.Current;

                _currentEnumerator.Dispose();
                _currentEnumerator = null;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            T? item;
            while ((item = Dequeue()) is not null)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
