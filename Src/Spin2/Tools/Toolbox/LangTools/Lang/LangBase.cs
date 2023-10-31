using System.Collections;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;


public interface ILangBase<T>
{
    Guid Id { get; }
    List<T> Children { get; }
    Cursor<T> CreateCursor();
}


public class LangBase<T> : ILangBase<T>, IEnumerable<T>
{
    private readonly List<T> _children = new List<T>();

    public Guid Id { get; } = Guid.NewGuid();
    public List<T> Children => _children;

    public Cursor<T> CreateCursor() => new Cursor<T>(_children);
    public void Add(T node) => _children.Add(node.NotNull());
    public void AddRange(IEnumerable<T> nodes) => _children.AddRange(nodes.NotNull());

    public IEnumerator<T> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
