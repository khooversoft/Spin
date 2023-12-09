using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;


public interface ILangBase<T>
{
    Guid Id { get; }
    Sequence<T> Children { get; }
    Cursor<T> CreateCursor();
    IEnumerable<T> GetChildrenRecursive();
}


public class LangBase<T> : ILangBase<T>, IEnumerable<T>
{
    private readonly Sequence<T> _children = new Sequence<T>();

    public Guid Id { get; } = Guid.NewGuid();
    public Sequence<T> Children => _children;
    public int Count => _children.Count;

    public Cursor<T> CreateCursor() => new Cursor<T>(_children);
    public void Add(T node) => _children.Add(node.NotNull());
    public void AddRange(IEnumerable<T> nodes) => _children.AddRange(nodes.NotNull());

    public IEnumerable<T> GetChildrenRecursive()
    {
        var stack = new Stack<ILangBase<T>>(new[] { this });

        while (stack.TryPop(out var root))
        {
            root.Children.OfType<ILangBase<T>>().ForEach(x => stack.Push(x));
            foreach (var child in root.Children) yield return child;
        }
    }

    public IEnumerator<T> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
