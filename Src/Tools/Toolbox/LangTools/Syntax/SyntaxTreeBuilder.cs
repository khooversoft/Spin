using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.LangTools;

public class SyntaxTreeBuilder
{
    public IMetaSyntax MetaSyntax { get; init; } = null!;
    public Sequence<ISyntaxTree> Children { get; } = new Sequence<ISyntaxTree>();
}


public static class SyntaxTreeBuilderExtensions
{
    public static SyntaxTree ConvertTo(this SyntaxTreeBuilder subject) => new SyntaxTree
    {
        MetaSyntaxName = subject.MetaSyntax?.Name,
        Children = subject.Children.ToImmutableArray(),
    };
}
