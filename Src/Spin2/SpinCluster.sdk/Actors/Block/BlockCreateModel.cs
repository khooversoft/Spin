using Toolbox.Block;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Block;

[GenerateSerializer, Immutable]
public record BlockCreateModel
{
    [Id(0)] public string ObjectId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public IReadOnlyList<BlockAccess> BlockAccess { get; init; } = Array.Empty<BlockAccess>();
}


public static class BlockCreateModelExtensions
{
    public static IValidator<BlockCreateModel> Validator { get; } = new Validator<BlockCreateModel>()
        .RuleFor(x => x.ObjectId).ValidObjectId()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.BlockAccess).NotNull()
        .RuleForEach(x => x.BlockAccess).Validate(BlockAccessValidator.Validator)
        .Build();

    public static Option Validate(this BlockCreateModel subject) => Validator.Validate(subject).ToOptionStatus();
}