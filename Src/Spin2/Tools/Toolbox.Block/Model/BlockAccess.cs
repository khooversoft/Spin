using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public sealed record BlockAccessRequest
{
    public bool WriteGrant { get; init; }
    public string? Claim { get; init; }

    public bool HasAccess(BlockAccess access)
    {
        return access is BlockAccess &&
            WriteGrant == access.WriteGrant &&
            Claim == access.Claim;
    }
}


public sealed record BlockAccess
{
    public bool WriteGrant { get; init; }
    public string? Claim { get; init; }
    public string BlockType { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
}

public static class BlockAccessValidator
{
    public static IValidator<BlockAccess> Validator { get; } = new Validator<BlockAccess>()
        .RuleFor(x => x.BlockType).ValidName()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .Build();

    public static ValidatorResult Validate(this BlockAccess subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static BlockAccess Verify(this BlockAccess subject, ScopeContextLocation location)
    {
        Validator.Validate(subject, location).ThrowOnError();
        return subject;
    }
}