using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

[GenerateSerializer, Immutable]
public record ContractQuery
{
    [Id(0)] public string PrincipalId { get; init; } = null!;
    [Id(1)] public string? BlockType { get; init; } = null!;
    [Id(2)] public bool LatestOnly { get; init; }

    public static ContractQuery CreateQuery<T>(string principalId, bool latestOnly) => new ContractQuery
    {
        PrincipalId = principalId,
        BlockType = typeof(T).GetTypeName(),
        LatestOnly = latestOnly,
    };

    public static IValidator<ContractQuery> Validator { get; } = new Validator<ContractQuery>()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
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
}