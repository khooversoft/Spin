using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;


[DebuggerDisplay("SyntaxNode: Name={Name}, Type={Type}, Children.Count={Children.Count}, Index={Index}")]
public sealed record SyntaxNode : ISyntaxNode
{
    public string Name { get; init; } = null!;
    public Sequence<ISyntaxNode> Children { get; init; } = new Sequence<ISyntaxNode>();
    public int? Index { get; init; }

    public bool Equals(SyntaxNode? obj)
    {
        bool result = obj is SyntaxNode subject &&
            Name == subject.Name &&
            Enumerable.SequenceEqual(Children, subject.Children);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Children, Index);

    public override string ToString() => $"ProductionRule [ Name={Name}, Index={Index}, ChildrenCount={Children.Count} ]".ToEnumerable()
        .Concat(Children.Select(x => x.ToString()))
        .Join(Environment.NewLine);
}
