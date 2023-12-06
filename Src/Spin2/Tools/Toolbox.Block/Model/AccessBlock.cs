using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

[Flags]
public enum BlockGrant
{
    None = 0x0,
    Read = 0x1,
    Write = 0x2,
    ReadWrite = Read | Write,
}

public sealed record AccessBlock
{
    public BlockGrant Grant { get; init; }
    public string? Claim { get; init; }
    public string BlockType { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;

    public bool Equals(AccessBlock? obj) => obj is AccessBlock document &&
        Grant == document.Grant &&
        Claim.ToNullIfEmpty() == document.Claim.ToNullIfEmpty() &&
        BlockType == document.BlockType &&
        PrincipalId == document.PrincipalId;

    public override int GetHashCode() => HashCode.Combine(Grant, Claim, BlockType, PrincipalId);

    public static IValidator<AccessBlock> Validator { get; } = new Validator<AccessBlock>()
        .RuleFor(x => x.Grant).Must(x => x.IsEnumValid<BlockGrant>(), _ => "Invalid block grant")
        .RuleFor(x => x.Claim).Must(x => x.IsEmpty() || IdPatterns.IsName(x), _ => "Invalid claim")
        .RuleFor(x => x.BlockType).ValidName()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .Build();

    public static AccessBlock Create<T>(BlockGrant blockGrant, string principalId, string? claim = null) => new AccessBlock
    {
        Grant = blockGrant,
        PrincipalId = principalId,
        BlockType = typeof(T).GetTypeName(),
        Claim = claim,
    };
}

public static class BlockAccessValidator
{
    public static Option Validate(this AccessBlock subject) => AccessBlock.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AccessBlock subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static bool HasAccess(this AccessBlock subject, string principalId, BlockGrant grant, string blockType) =>
        subject.Grant.HasFlag(grant) &&
        subject.PrincipalId == principalId.Assert(x => IdPatterns.IsPrincipalId(x), "Invalid principalId") &&
        subject.BlockType == blockType.Assert(x => IdPatterns.IsName(x), "Invalid block type");
}
