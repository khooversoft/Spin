namespace Toolbox.Store;

public class HybridCacheCounters
{
    private long _hits;
    private long _misses;
    private long _setCount;
    private long _setFailCount;
    private long _deleteCount;
    private long _retireCount;

    public void Clear()
    {
        _hits = 0;
        _misses = 0;
        _setCount = 0;
        _setFailCount = 0;
        _deleteCount = 0;
        _retireCount = 0;
    }

    public long Hits => Interlocked.Read(ref _hits);
    public long Misses => Interlocked.Read(ref _misses);
    public long SetCount => Interlocked.Read(ref _setCount);
    public long SetFailCount => Interlocked.Read(ref _setFailCount);
    public long DeleteCount => Interlocked.Read(ref _deleteCount);
    public long RetireCount => Interlocked.Read(ref _retireCount);

    public void AddHits(int value = 1) => Interlocked.Add(ref _hits, value);
    public void AddMisses(int value = 1) => Interlocked.Add(ref _misses, value);
    public void AddSetCount(int value = 1) => Interlocked.Add(ref _setCount, value);
    public void AddSetFailCount(int value = 1) => Interlocked.Add(ref _setFailCount, value);
    public void AddDeleteCount(int value = 1) => Interlocked.Add(ref _deleteCount, value);
    public void AddRetireCount(int value = 1) => Interlocked.Add(ref _retireCount, value);
}
