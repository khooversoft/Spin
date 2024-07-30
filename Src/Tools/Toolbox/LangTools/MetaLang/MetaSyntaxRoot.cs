using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.LangTools;

public record MetaSyntaxRoot
{
    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
    public ProductionRule Rule { get; init; } = null!;

    public IReadOnlyDictionary<string, IMetaSyntax> Nodes { get; init; } = ImmutableDictionary<string, IMetaSyntax>.Empty;
}


public static class MetaSyntaxRootExtensions
{
    public static MetaSyntaxRoot ConvertTo(this MetaParserContext subject, Option option) => new MetaSyntaxRoot
    {
        StatusCode = option.StatusCode,
        Error = option.Error,
        Rule = subject.RootRule,
        Nodes = subject.Nodes.ToImmutableDictionary(),
    };
}