namespace Toolbox.Data;

public class DataProviderCounters
{
    private long _hits;
    private long _misses;
    private long _appendCount;
    private long _appendFailCount;
    private long _setCount;
    private long _setFailCount;
    private long _deleteCount;
    private long _deleteFailCount;
    private long _retireCount;

    public void Clear()
    {
        _hits = 0;
        _misses = 0;
        _appendCount = 0;
        _appendFailCount = 0;
        _setCount = 0;
        _setFailCount = 0;
        _deleteCount = 0;
        _deleteFailCount = 0;
        _retireCount = 0;
    }

    public long Hits => Interlocked.Read(ref _hits);
    public long Misses => Interlocked.Read(ref _misses);
    public long AppendCount => Interlocked.Read(ref _appendCount);
    public long AppendFailCount => Interlocked.Read(ref _appendFailCount);
    public long SetCount => Interlocked.Read(ref _setCount);
    public long SetFailCount => Interlocked.Read(ref _setFailCount);
    public long DeleteCount => Interlocked.Read(ref _deleteCount);
    public long DeleteFailCount => Interlocked.Read(ref _deleteFailCount);
    public long RetireCount => Interlocked.Read(ref _retireCount);

    public void AddHits(int value = 1) => Interlocked.Add(ref _hits, value);
    public void AddMisses(int value = 1) => Interlocked.Add(ref _misses, value);
    public void AddAppendCount(int value = 1) => Interlocked.Add(ref _appendCount, value);
    public void AddAppendFailCount(int value = 1) => Interlocked.Add(ref _appendFailCount, value);
    public void AddSetCount(int value = 1) => Interlocked.Add(ref _setCount, value);
    public void AddSetFailCount(int value = 1) => Interlocked.Add(ref _setFailCount, value);
    public void AddDeleteCount(int value = 1) => Interlocked.Add(ref _deleteCount, value);
    public void AddDeleteFailCount(int value = 1) => Interlocked.Add(ref _deleteFailCount, value);
    public void AddRetireCount(int value = 1) => Interlocked.Add(ref _retireCount, value);
}
