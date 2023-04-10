using System;
using System.Diagnostics;

namespace Toolbox.Actor.Tools
{
    [DebuggerDisplay("Key={Key}, AccessedCount={AccessedCount}, LastAccessed={LastAccessed}")]
    public class CacheItem<TKey, T>
    {
        internal CacheItem(TKey key, T value)
        {
            Key = key;
            Value = value;
            LastAccessed = DateTimeOffset.UtcNow;
            AccessedCount = 1;
        }

        public TKey Key;

        public T Value;

        public DateTimeOffset LastAccessed { get; private set; }

        public int AccessedCount { get; private set; }

        public void RecordAccessed()
        {
            LastAccessed = DateTimeOffset.UtcNow;
            AccessedCount++;
        }
    }
}

