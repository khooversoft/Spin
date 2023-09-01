using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

[GenerateSerializer, Immutable]
public sealed record ContractCreateModel
{
    [Id(0)] public string DocumentId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public IReadOnlyList<AccessBlock> BlockAccess { get; init; } = Array.Empty<AccessBlock>();
    [Id(3)] public IReadOnlyList<RoleAccessBlock> RoleRights { get; init; } = Array.Empty<RoleAccessBlock>();
}


public static class BlockCreateModelExtensions
{
    public static IValidator<ContractCreateModel> Validator { get; } = new Validator<ContractCreateModel>()
        .RuleFor(x => x.DocumentId).ValidResourceId(ResourceType.Account)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.BlockAccess).NotNull()
        .RuleForEach(x => x.BlockAccess).Validate(BlockAccessValidator.Validator)
        .RuleFor(x => x.RoleRights).NotNull()
        .RuleForEach(x => x.RoleRights).Validate(BlockRoleAccessValidator.Validator)
        .Build();

    public static Option Validate(this ContractCreateModel subject) => Validator.Validate(subject).ToOptionStatus();
}