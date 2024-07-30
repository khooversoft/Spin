using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.LangTools;

public interface ISyntaxNode
{
    string Name { get; }
    int? Index { get; }
}


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

public sealed record TerminalNode : ISyntaxNode
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public int? Index { get; init; }

    public bool Equals(TerminalNode? obj)
    {
        bool result = obj is TerminalNode subject &&
            Name == subject.Name &&
            Text == subject.Text;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Text, Index);
    public override string ToString() => $"TerminalNode [ Name={Name}, Text={Text}, Index={Index} ]";
}