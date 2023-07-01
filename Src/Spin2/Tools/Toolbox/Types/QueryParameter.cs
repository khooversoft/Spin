using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public record QueryParameter
{
    public int Index { get; init; } = 0;
    public int Count { get; init; } = 1000;
    public string? Filter { get; init; }
    public bool Recurse { get; init; }

    public static QueryParameter Default { get; } = new QueryParameter();
}


public static class QueryParameterExtensions
{
    public static string ToQueryString(this QueryParameter subject)
    {
        subject.NotNull();

        return new string?[]
        {
            subject.Filter?.ToString()?.Func(x => $"filter={Uri.EscapeDataString(x)}"),
            $"index={subject.Index}",
            $"count={subject.Count}",
            $"recurse={subject.Recurse}",
        }
        .Where(x => x != null)
        .Join('&');
    }
}
