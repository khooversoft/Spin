using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GraphLink
{
    public string NodeKey { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string FileId { get; init; } = null!;

    public override string ToString() => $"NodeKey={NodeKey}, Name={Name}, FileId={FileId}";

    public static IValidator<GraphLink> Validator { get; } = new Validator<GraphLink>()
        .RuleFor(x => x.NodeKey).NotEmpty()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.FileId).Must(x => IdPatterns.IsPath(x), x => $"Invalid File path={x}")
        .Build();
}

public static class GraphDataLinkTool
{
    public static Option Validate(this GraphLink subject) => GraphLink.Validator.Validate(subject).ToOptionStatus();

    public static string ToDataMapString(this IEnumerable<KeyValuePair<string, GraphLink>> subject) => subject.NotNull()
        .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
        .Select(x => $"{x.Key}={x.Value}")
        .Join(',');
}
