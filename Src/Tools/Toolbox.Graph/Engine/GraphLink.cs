using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GraphLink
{
    public string NodeKey { get; init; } = null!;
    public string Name { get; init; } = null!;

    public string FileId { get; init; } = null!;

    public static IValidator<GraphLink> Validator { get; } = new Validator<GraphLink>()
        .RuleFor(x => x.NodeKey).NotEmpty()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.FileId).Must(x => IdPatterns.IsPath(x), x => $"Invalid File path={x}")
        .Build();
}

public static class GraphDataLinkTool
{
    public static Option Validate(this GraphLink subject) => GraphLink.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphLink subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ImmutableDictionary<string, GraphLink> Empty { get; } = ImmutableDictionary<string, GraphLink>.Empty;

    public static bool DeepEquals(this IEnumerable<KeyValuePair<string, GraphLink>> source, IEnumerable<KeyValuePair<string, GraphLink>> target)
    {
        if (source == null && target == null) return true;
        if (source == null || target == null) return false;

        var sourceList = source.OrderBy(x => x.Key).ToArray();
        var targetList = target.OrderBy(x => x.Key).ToArray();
        if (sourceList.Length != targetList.Length) return false;

        var zip = sourceList.Zip(targetList, (x, y) => (source: x, target: y));
        var isEqual = zip.All(x => x.source.Key.Equals(x.target.Key, StringComparison.OrdinalIgnoreCase) switch
        {
            false => false,
            true => (x.source.Value, x.target.Value) switch
            {
                (null, null) => true,
                (GraphLink s1, GraphLink s2) => (s1 == s2),
                _ => false,
            }
        });

        return isEqual;
    }

    public static string ToDataMapString(this IEnumerable<KeyValuePair<string, GraphLink>> subject)
    {
        subject.NotNull();
        return subject
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => $"{x.Key}={x.Value}")
            .Join(',');
    }
}