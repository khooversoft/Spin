using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public record GenesisBlock
{
    public static string BlockType { get; } = "genesis";

    public string Type { get; init; } = BlockType;
    public string DocumentId { get; init; } = null!;
    public string OwnerPrincipalId { get; init; } = null!;
}


public static class GenesisBlockValidator
{
    public static IValidator<GenesisBlock> Validator { get; } = new Validator<GenesisBlock>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.DocumentId).ValidResourceId(ResourceType.DomainOwned)
        .RuleFor(x => x.OwnerPrincipalId).ValidResourceId(ResourceType.Principal)
        .Build();

    public static Option Validate(this GenesisBlock subject) => Validator.Validate(subject).ToOptionStatus();
}