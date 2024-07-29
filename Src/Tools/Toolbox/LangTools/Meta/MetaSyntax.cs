using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.LangTools;

public interface IMetaSyntax
{
    string Name { get; }
    public int? Index { get; }
}

[DebuggerDisplay("TerminalSymbol: Name={Name}, Text={Text}, Regex={Regex}, Index={Index}")]
public sealed record TerminalSymbol : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public bool Regex { get; init; }
    public int? Index { get; init; }

    public bool Equals(TerminalSymbol? obj)
    {
        bool result = obj is TerminalSymbol subject &&
            Name == subject.Name &&
            Text == subject.Text &&
            Regex == subject.Regex;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Text, Regex, Index);
    public override string ToString() => $"TerminalSymbol [ Name={Name}, Text={Text}, Regex={Regex}, Index={Index} ]";
}

[DebuggerDisplay("VirtualTerminalSymbol: Name={Name}, Text={Text}, Index={Index}")]
public sealed record VirtualTerminalSymbol : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public int? Index { get; init; }

    public bool Equals(VirtualTerminalSymbol? obj)
    {
        bool result = obj is VirtualTerminalSymbol subject &&
            Name == subject.Name &&
            Text == subject.Text;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Text, Index);
    public override string ToString() => $"VirtualTerminalSymbol [ Name={Name}, Index={Index} ]";
}


[DebuggerDisplay("GroupOperator: Name={Name}, Text={Text}, Index={Index}")]
public sealed record GroupOperator : IMetaSyntax
{
    public string Name { get; init; } = null!;
    public string Text { get; init; } = null!;
    public int? Index { get; init; }

    public bool Equals(GroupOperator? obj)
    {
        bool result = obj is GroupOperator subject &&
            Name == subject.Name &&
            Text == subject.Text;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Name, Text, Index);
    public override string ToString() => $"GroupOperator [ Name={Name}, Index={Index} ]";
}