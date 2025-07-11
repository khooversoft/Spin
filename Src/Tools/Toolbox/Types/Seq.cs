using Toolbox.Tools;

namespace Toolbox.Types;

public class Seq<T>
{
    private const string _emptyListText = "Empty list";
    private readonly object _lock = new object();
    private readonly IReadOnlyList<T> _list;
    private int _index = -1;

    public Seq() => _list = Array.Empty<T>();
    public Seq(IEnumerable<T> list) => _list = [.. list.NotNull()];

    public int Count => _list.Count;

    public void Start()
    {
        Count.Assert(x => x > 0, _emptyListText);
        lock (_lock) { _index = -1; }
    }

    public void End()
    {
        Count.Assert(x => x > 0, _emptyListText);
        lock (_lock) { _index = Count; }
    }

    public T Index()
    {
        lock (_lock)
        {
            Count.Assert(x => x > 0, _emptyListText);
            _index.Assert(x => x >= 0 && x < Count, "Invalid index");
            return _list[_index];
        }
    }

    public T First()
    {
        lock (_lock)
        {
            Count.Assert(x => x > 0, _emptyListText);
            _index = 0;
            return _list[_index];
        }
    }

    public T Last()
    {
        lock (_lock)
        {
            Count.Assert(x => x > 0, _emptyListText);
            _index = _list.Count - 1;
            return _list[_index];
        }
    }

    public T Next()
    {
        lock (_lock)
        {
            Count.Assert(x => x > 0, _emptyListText);
            _index = Math.Min(_index + 1, Count);

            _index.Assert(x => x < Count, "No next value");
            return _list[_index];
        }
    }

    public T Back()
    {
        lock (_lock)
        {
            Count.Assert(x => x > 0, _emptyListText);
            _index = Math.Max(_index - 1, -1);

            _index.Assert(x => x >= 0, "No previous value");
            return _list[_index];
        }
    }

    public bool TryGetNext(out T? value)
    {
        value = default;
        if (Count == 0) return false;

        lock (_lock)
        {
            _index = Math.Min(_index + 1, Count);
            if (_index >= Count) return false;

            value = _list[_index];
            return true;
        }
    }

    public bool TryGetPrevious(out T? value)
    {
        value = default;
        if (Count == 0) return false;

        lock (_lock)
        {
            _index = Math.Max(_index - 1, -1);
            if (_index <= -1) return false;

            value = _list[_index];
            return true;
        }
    }
}


public static class SeqTool
{
    public static Seq<T> ToSeq<T>(this IEnumerable<T> list) => new Seq<T>(list);
}