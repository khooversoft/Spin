using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public interface IPatternBase<T>
{
    Guid Id { get; }
    List<T> Children { get; }
    Cursor<T> CreateCursor();
}


public abstract class PatternBase : PatternBase<IPatternSyntax>, IPatternSyntax
{
    public PatternBase(string? name) => Name = name;
    public string? Name { get; }
    public abstract Option<Sequence<IPatternSyntax>> Process(PatternContext pContext);
}

public class PatternBase<T> : IPatternBase<T>, IEnumerable<T>
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
