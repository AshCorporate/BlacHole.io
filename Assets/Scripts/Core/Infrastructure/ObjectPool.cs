using System;
using System.Collections.Generic;

namespace BlacHole.Core.Infrastructure
{
    /// <summary>
    /// Generic object pool. Reuse instances to avoid GC pressure.
    /// </summary>
    public class ObjectPool<T>
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly int _maxSize;

        public int CountInactive => _pool.Count;

        public ObjectPool(Func<T> factory,
                          Action<T> onGet = null,
                          Action<T> onRelease = null,
                          int defaultCapacity = 16,
                          int maxSize = 1000)
        {
            _factory    = factory   ?? throw new ArgumentNullException(nameof(factory));
            _onGet      = onGet;
            _onRelease  = onRelease;
            _maxSize    = maxSize;
            _pool       = new Stack<T>(defaultCapacity);
        }

        /// <summary>Retrieve an instance (creates one if pool is empty).</summary>
        public T Get()
        {
            T item = _pool.Count > 0 ? _pool.Pop() : _factory();
            _onGet?.Invoke(item);
            return item;
        }

        /// <summary>Return an instance to the pool.</summary>
        public void Release(T item)
        {
            _onRelease?.Invoke(item);
            if (_pool.Count < _maxSize)
                _pool.Push(item);
        }

        /// <summary>Pre-warm the pool with N instances.</summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = _factory();
                _onRelease?.Invoke(item);
                if (_pool.Count < _maxSize)
                    _pool.Push(item);
            }
        }

        public void Clear() => _pool.Clear();
    }
}
