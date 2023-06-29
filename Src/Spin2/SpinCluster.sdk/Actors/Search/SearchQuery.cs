using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Search;

[GenerateSerializer, Immutable]
public record SearchQuery
{
    [Id(0)] public int Index { get; init; } = 0;
    [Id(1)] public int Count { get; init; } = 1000;
    [Id(2)] public string Filter { get; init; } = null!;
    [Id(3)] public bool Recursive { get; init; }

    public static QueryParameter Default { get; } = new QueryParameter();
}


public static class SearchQueryValidator
{
    public static Validator<SearchQuery> Validator { get; } = new Validator<SearchQuery>()
        .RuleFor(x => x.Filter).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this SearchQuery subject) => Validator.Validate(subject);
    public static ValidatorResult Validate(this SearchQuery subject, ScopeContext context) => Validator.Validate(subject);

    public static QueryParameter ConvertTo(this SearchQuery subject) => new QueryParameter
    {
        Index = subject.Index,
        Count = subject.Count,
        Filter = subject.Filter,
        Recursive = subject.Recursive,
    };
}
