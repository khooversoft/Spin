using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public record GenesisBlock
{
    public static string BlockType { get; } = "genesis";

    public string Type { get; init; } = BlockType;
    public string ObjectId { get; init; } = null!;
    public string OwnerPrincipalId { get; init; } = null!;
}


public static class GenesisBlockValidator
{
    private const string _noErrorText = "< no error message >";

    public static IValidator<GenesisBlock> Validator { get; } = new Validator<GenesisBlock>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.ObjectId).ValidObjectId()
        .RuleFor(x => x.OwnerPrincipalId).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this GenesisBlock subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}