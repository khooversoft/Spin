using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.LangTools;

public interface ISyntaxTree
{
}


public sealed record SyntaxTree : ISyntaxTree
{
    public IMetaSyntax? MetaSyntax { get; init; }
    public IReadOnlyList<ISyntaxTree> Children { get; init; } = Array.Empty<ISyntaxTree>();

    public bool Equals(SyntaxTree? obj)
    {
        bool result = obj is SyntaxTree subject &&
            ((MetaSyntax == null && subject.MetaSyntax == null) || MetaSyntax?.Equals(subject.MetaSyntax) == true) &&
            Enumerable.SequenceEqual(Children, subject.Children);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(MetaSyntax, Children);
}
