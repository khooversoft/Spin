using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GraphDataLink
{
    public string Name { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public string Schema { get; init; } = null!;
    public string FileId { get; init; } = null!;

    public static IValidator<GraphDataLink> Validator { get; } = new Validator<GraphDataLink>()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.TypeName).ValidName()
        .RuleFor(x => x.Schema).ValidName()
        .RuleFor(x => x.FileId).Must(x => IdPatterns.IsPath(x), x => $"Invalid File path={x}")
        .Build();
}

public static class GraphDataLinkTool
{
    public static Option Validate(this GraphDataLink subject) => GraphDataLink.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphDataLink subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ImmutableDictionary<string, GraphDataLink> Empty { get; } = ImmutableDictionary<string, GraphDataLink>.Empty;

    public static bool DeepEquals(this IEnumerable<KeyValuePair<string, GraphDataLink>> source, IEnumerable<KeyValuePair<string, GraphDataLink>> target)
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
                (GraphDataLink s1, GraphDataLink s2) => (s1 == s2),
                _ => false,
            }
        });

        return isEqual;
    }

    public static string ToDataMapString(this IEnumerable<KeyValuePair<string, GraphDataLink>> subject)
    {
        subject.NotNull();
        return subject
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => $"{x.Key}={x.Value}")
            .Join(',');
    }
}