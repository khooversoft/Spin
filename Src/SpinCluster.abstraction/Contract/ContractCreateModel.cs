﻿using Toolbox.Block;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record ContractCreateModel
{
    [Id(0)] public string DocumentId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public IReadOnlyList<AccessBlock> BlockAccess { get; init; } = Array.Empty<AccessBlock>();
    [Id(3)] public IReadOnlyList<RoleAccessBlock> RoleRights { get; init; } = Array.Empty<RoleAccessBlock>();

    public static IValidator<ContractCreateModel> Validator { get; } = new Validator<ContractCreateModel>()
        .RuleFor(x => x.DocumentId).ValidResourceId(ResourceType.DomainOwned)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.BlockAccess).NotNull()
        .RuleForEach(x => x.BlockAccess).Validate(AccessBlock.Validator)
        .RuleFor(x => x.RoleRights).NotNull()
        .RuleForEach(x => x.RoleRights).Validate(BlockRoleAccessValidator.Validator)
        .Build();
}


public static class BlockCreateModelExtensions
{
    public static Option Validate(this ContractCreateModel subject) => ContractCreateModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ContractCreateModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}