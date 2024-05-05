using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GraphDataSource
{
    public string Name { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public string Schema { get; init; } = null!;
    public string Data64 { get; init; } = null!;

    public static IValidator<GraphDataSource> Validator { get; } = new Validator<GraphDataSource>()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.TypeName).ValidName()
        .RuleFor(x => x.Schema).ValidName()
        .RuleFor(x => x.Data64).Must(x => Base64.IsValid(x), x => $"Invalid base 64 data={x}")
        .Build();
}

public static class GraphDataSourceTool
{
    public static Option Validate(this GraphDataSource subject) => GraphDataSource.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this GraphDataSource subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ImmutableDictionary<string, GraphDataSource> Empty { get; } = ImmutableDictionary<string, GraphDataSource>.Empty;

    public static bool DeepEquals(this IEnumerable<KeyValuePair<string, GraphDataSource>> source, IEnumerable<KeyValuePair<string, GraphDataSource>> target)
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
                (GraphDataSource s1, GraphDataSource s2) => (s1 == s2),
                _ => false,
            }
        });

        return isEqual;
    }

    public static string ToDataMapString(this IEnumerable<KeyValuePair<string, GraphDataSource>> subject) => subject.NotNull()
        .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
        .Select(x => $"{x.Key}={x.Value}")
        .Join(',');
}