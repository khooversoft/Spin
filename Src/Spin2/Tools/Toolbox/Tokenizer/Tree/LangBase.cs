using System.Collections;
using Toolbox.Types;

namespace Toolbox.Tokenizer.Tree;

public enum LangType
{
    Root,
    SyntaxToken,
    Value,
}


public interface ILangBase<T>
{
    Guid Id { get; }
    LangType Type { get; }
    IList<T> Children { get; }
    Cursor<T> CreateCursor();
}


public class LangBase<T> : ILangBase<T>, IEnumerable<T>
{
    private readonly List<T> _children = new List<T>();

    public LangBase() { }
    public LangBase(LangType type) => Type = type;

    public Guid Id { get; } = Guid.NewGuid();
    public LangType Type { get; } = LangType.Root;

    public IList<T> Children => _children;
    public Cursor<T> CreateCursor() => new Cursor<T>(_children);

    public void Add(T node) => _children.Add(node);

    public IEnumerator<T> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
