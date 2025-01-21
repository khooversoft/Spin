namespace Toolbox.Tools;

public class MarkTime
{
    private readonly long _msDuration;
    private readonly object _lock = new object();
    private DateTime _date;
    private long _count;

    public MarkTime(long msDuration)
    {
        _msDuration = msDuration;
        _date = DateTime.Now.AddMilliseconds(_msDuration);
    }

    public MarkTime(TimeSpan timeSpan)
    {
        _msDuration = (long)timeSpan.TotalMilliseconds;
        _date = DateTime.Now.AddMilliseconds(_msDuration);
    }

    public long Count => _count;

    public bool IsPass()
    {
        lock (_lock)
        {
            if (DateTime.Now < _date) return false;

            _date = DateTime.Now.AddMilliseconds(_msDuration);
            _count++;
            return true;
        }
    }
}
