using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbox.Tools
{
    public class BoundedQueue<T> : IEnumerable<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly int _maxSize;

        public BoundedQueue(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Enqueue(T value)
        {
            _queue.Enqueue(value);
            while (_queue.Count > _maxSize) _queue.TryDequeue(out T _);
        }

        public bool TryDequeue(out T result) => _queue.TryDequeue(out result);

        public IEnumerator<T> GetEnumerator() => _queue.ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
