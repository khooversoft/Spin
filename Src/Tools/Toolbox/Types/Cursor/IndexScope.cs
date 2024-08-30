using Toolbox.Tools;

namespace Toolbox.Types;

public class IndexScope<T>
{
    private readonly Cursor<T> _cursor;
    private readonly Stack<int> _positionStack = new Stack<int>();

    public IndexScope(Cursor<T> cursor) => _cursor = cursor;

    public FinalizeScope<IndexScope<T>> PushWithScope()
    {
        Push();
        return new FinalizeScope<IndexScope<T>>(this, x => x.Pop(), x => RemovePush());
    }

    public void Push() => _positionStack.Push(_cursor.Index);

    public void Pop()
    {
        _positionStack.TryPop(out var position).Assert(x => x == true, "Empty stack");
        _cursor.Index = position;
    }

    public void RemovePush() => _positionStack.TryPop(out var _).Assert(x => x == true, "Empty stack");
}
