using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public record ContractQuery
{
    [Id(0)] public string PrincipalId { get; init; } = null!;
    [Id(1)] public IReadOnlyList<QueryBlockType> BlockTypes { get; init; } = Array.Empty<QueryBlockType>();


    public static ContractQuery CreateQuery<T>(string principalId, bool latestOnly = false) => new ContractQuery
    {
        PrincipalId = principalId,
        BlockTypes = new QueryBlockType
        {
            BlockType = typeof(T).GetTypeName(),
            LatestOnly = latestOnly,
        }.ToEnumerable().ToArray(),
    };

    public static IValidator<ContractQuery> Validator { get; } = new Validator<ContractQuery>()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleForEach(x => x.BlockTypes).Validate(QueryBlockType.Validator)
        .Build();
}


[GenerateSerializer, Immutable]
public record QueryBlockType
{
    [Id(0)] public string BlockType { get; init; } = null!;
    [Id(1)] public bool LatestOnly { get; init; }

    public static IValidator<QueryBlockType> Validator { get; } = new Validator<QueryBlockType>()
        .RuleFor(x => x.BlockType).Must(x => x.IsEmpty() || IdPatterns.IsName(x), _ => "Invalid block type")
        .Build();
}

public static class ContractQueryExtensions
{
    public static Option Validate(this ContractQuery subject) => ContractQuery.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ContractQuery subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static string? GetBlockTypes(this ContractQuery subject) => subject.BlockTypes switch
    {
        { Count: 0 } => null,
        var v => v.Select(x => x.BlockType).Join(';'),
    };

    public static bool LatestOnly(this ContractQuery subject, string blockType) => subject.BlockTypes
        .Any(x => x.BlockType == blockType && x.LatestOnly);
}