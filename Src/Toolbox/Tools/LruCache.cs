using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Actor.Tools
{
    /// <summary>
    /// LRU (Least Recently Used) list.  The least used item are in the beginning of the list, while the most used
    /// are at the end of the list.
    /// 
    /// Enumerator for LRU Cache item (details accessed count and last date)
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="T">type to store</typeparam>
    public class LruCache<TKey, T> : IEnumerable<CacheItem<TKey, T>>
    {
        private readonly Dictionary<TKey, LinkedListNode<CacheItem<TKey, T>>> _cacheMap;
        private LinkedList<CacheItem<TKey, T>> _lruList = new LinkedList<CacheItem<TKey, T>>();
        private readonly object _lock = new object();

        public LruCache(int capacity)
        {
            Verify.Assert(capacity > 0, $"{nameof(capacity)} must be greater than 0");
            Capacity = capacity;

            _cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem<TKey, T>>>();
        }

        public LruCache(int capacity, IEqualityComparer<TKey> comparer)
            : this(capacity)
        {
            _cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem<TKey, T>>>(comparer);
        }

        /// <summary>
        /// Event when cache item has been removed
        /// </summary>
        public event Action<CacheItem<TKey, T>>? CacheItemRemoved;

        /// <summary>
        /// Get or set value based on key.  If key does not exist, return default(T)
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>default(T) or value</returns>
        public T this[TKey key]
        {
            get => TryGetValue(key, out T value) ? value : default!;
            set => Set(key, value);
        }

        /// <summary>
        /// Capacity of LRU cache (cannot be changed)
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Current count of cache
        /// </summary>
        public int Count { get { return _lruList.Count; } }

        /// <summary>
        /// Return values in LRU cache (least used to most used)
        /// </summary>
        public IEnumerable<T> GetValues()
        {
            lock (_lock)
            {
                return _lruList
                    .Select(x => x.Value)
                    .ToList();
            }
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        /// <returns>this</returns>
        public LruCache<TKey, T> Clear()
        {
            lock (_lock)
            {
                _cacheMap.Clear();
                _lruList.Clear();
            }

            return this;
        }

        /// <summary>
        /// Set cache item.  If value already exist, remove it and add the new one
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>this</returns>
        public LruCache<TKey, T> Set(TKey key, T value)
        {
            lock (_lock)
            {
                if (_cacheMap.TryGetValue(key, out LinkedListNode<CacheItem<TKey, T>> node))
                {
                    _lruList.Remove(node);
                }
                else
                {
                    while (_cacheMap.Count >= Capacity) RemoveFirst();

                    var cacheItem = new CacheItem<TKey, T>(key, value);
                    node = new LinkedListNode<CacheItem<TKey, T>>(cacheItem);
                }

                _lruList.AddLast(node);
                _cacheMap[key] = node;
            }

            return this;
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>true if removed, false if not</returns>
        public bool Remove(TKey key) => TryRemove(key, out T value);

        /// <summary>
        /// Try to get value from cache
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value to load if found</param>
        /// <param name="markUsed">true to mark cache item used, false will not</param>
        /// <returns>true if found, false if not</returns>
        public bool TryGetValue(TKey key, out T value, bool markUsed = true)
        {
            value = default!;
            lock (_lock)
            {
                if (_cacheMap.TryGetValue(key, out LinkedListNode<CacheItem<TKey, T>> node))
                {
                    value = node.Value.Value;

                    if (markUsed)
                    {
                        node.Value.RecordAccessed();
                        _lruList.Remove(node);
                        _lruList.AddLast(node);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to remove key, set out value
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value to set</param>
        /// <returns>true if removed, false if not</returns>
        public bool TryRemove(TKey key, out T value)
        {
            lock (_lock)
            {
                if (!_cacheMap.TryGetValue(key, out LinkedListNode<CacheItem<TKey, T>> node))
                {
                    value = default!;
                    return false;
                }

                _lruList.Remove(node);
                _cacheMap.Remove(key);

                value = node.Value.Value;
                return true;
            }
        }

        /// <summary>
        /// Get cache item details
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>cache item details or null if not found</returns>
        public CacheItem<TKey, T>? GetCacheDetails(TKey key)
        {
            lock (_lock)
            {
                LinkedListNode<CacheItem<TKey, T>> node;

                if (_cacheMap.TryGetValue(key, out node)) return node.Value;
            }

            return null;
        }

        /// <summary>
        /// Return enumerator of cache item details, from lease used to most used
        /// </summary>
        /// <returns>enumerator</returns>
        public IEnumerator<CacheItem<TKey, T>> GetEnumerator()
        {
            List<CacheItem<TKey, T>> list;

            lock (_lock)
            {
                list = new List<CacheItem<TKey, T>>(_lruList);
            }

            foreach (var item in list)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void RemoveFirst()
        {
            // Remove from LRUPriority
            LinkedListNode<CacheItem<TKey, T>> node = _lruList.First;
            _lruList.RemoveFirst();

            // Remove from cache
            _cacheMap.Remove(node.Value.Key);

            // Notify of event removed
            CacheItemRemoved?.Invoke(node.Value);
        }
    }
}
