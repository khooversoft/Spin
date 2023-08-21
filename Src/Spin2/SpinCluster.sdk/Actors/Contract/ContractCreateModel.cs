using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

[GenerateSerializer, Immutable]
public record ContractCreateModel
{
    [Id(0)] public string DocumentId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public IReadOnlyList<BlockAccess> BlockAccess { get; init; } = Array.Empty<BlockAccess>();
}


public static class BlockCreateModelExtensions
{
    public static IValidator<ContractCreateModel> Validator { get; } = new Validator<ContractCreateModel>()
        .RuleFor(x => x.DocumentId).ValidContractId()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.BlockAccess).NotNull()
        .RuleForEach(x => x.BlockAccess).Validate(BlockAccessValidator.Validator)
        .Build();

    public static Option Validate(this ContractCreateModel subject) => Validator.Validate(subject).ToOptionStatus();
}