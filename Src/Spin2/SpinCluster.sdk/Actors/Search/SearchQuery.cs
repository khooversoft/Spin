using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Search;

[GenerateSerializer, Immutable]
public record SearchQuery
{
    [Id(0)] public string Schema { get; init; } = null!;
    [Id(1)] public string Tenant { get; init; } = null!;
    [Id(2)] public string Filter { get; init; } = "/";
    [Id(3)] public int Index { get; init; } = 0;
    [Id(4)] public int Count { get; init; } = 1000;
    [Id(5)] public bool Recurse { get; init; }

    public static QueryParameter Default { get; } = new QueryParameter();
}


public static class SearchQueryValidator
{
    public static IValidator<SearchQuery> Validator { get; } = new Validator<SearchQuery>()
        .RuleFor(x => x.Schema).NotEmpty()
        .RuleFor(x => x.Tenant).NotEmpty()
        .RuleFor(x => x.Filter).NotEmpty()
        .Build();

    public static Option Validate(this SearchQuery subject) => Validator.Validate(subject).ToOptionStatus();

    public static QueryParameter ConvertTo(this SearchQuery subject) => new QueryParameter
    {
        Index = subject.Index,
        Count = subject.Count,
        Filter = $"{subject.Tenant}/{subject.Filter}",
        Recurse = subject.Recurse,
    };
}
