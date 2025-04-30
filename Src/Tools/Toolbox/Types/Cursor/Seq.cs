using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Types.Cursor;

public class Seq<T>
{
    private Cursor<T> _cursor;
    public Seq() => _cursor = new([]);
    public Seq(IEnumerable<T> list) => _cursor = new(list.ToArray());

    public int Count => _cursor.List.Count;

    public T First()
    {
        Count.Assert(x => x > 0, "Empty list");
        _cursor.Index = 0;
        return _cursor.Current;
    }

    public T Last()
    {
        Count.Assert(x => x > 0, "Empty list");
        _cursor.Index = Count - 1;
        return _cursor.Current;
    }

    public T Next()
    {
        _cursor.TryGetValue(out var value).BeTrue("No value");
        _cursor.Index++;
        return _cursor.Current;
    }

    public T Previous()
    {
        Count.Assert(x => x > 0, "Empty list");
        _cursor.Index--;
        return _cursor.Current;
    }
}
