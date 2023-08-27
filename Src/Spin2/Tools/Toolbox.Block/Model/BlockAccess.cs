using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

[Flags]
public enum BlockGrant
{
    None = 0x0,
    Read = 0x1,
    Write = 0x2,
    ReadWrite = Read | Write,
    Owner = 0x100,
}

public sealed record BlockAccess
{
    public BlockGrant Grant { get; init; } = BlockGrant.None;
    public string? Claim { get; init; }
    public required string BlockType { get; init; }
    public required string PrincipalId { get; init; }
}

public static class BlockAccessValidator
{
    public static IValidator<BlockAccess> Validator { get; } = new Validator<BlockAccess>()
        .RuleFor(x => x.Grant).Must(x => x.IsEnumValid<BlockGrant>(), _ => "Invalid block grant")
        .RuleFor(x => x.Claim).Must(x => x.IsEmpty() || IdPatterns.IsName(x), _ => "Invalid claim")
        .RuleFor(x => x.BlockType).ValidBlockType()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .Build();

    public static bool HasAccess(this BlockAccess subject, string principalId, BlockGrant grant, string blockType) =>
        subject.Grant.HasFlag(grant) &&
        subject.PrincipalId == principalId.Assert(x => IdPatterns.IsPrincipalId(x), "Invalid principalId") &&
        subject.BlockType == blockType.Assert(x => IdPatterns.IsBlockType(x), "Invalid block type");

    public static bool HasAccess(this BlockAccess subject, string principalId, BlockGrant grant) =>
        subject.Grant.HasFlag(grant) &&
        subject.PrincipalId == principalId.Assert(x => IdPatterns.IsPrincipalId(x), "Invalid principalId");
}