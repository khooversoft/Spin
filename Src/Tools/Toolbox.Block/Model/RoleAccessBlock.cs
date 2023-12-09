using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

[Flags]
public enum BlockRoleGrant
{
    None = 0x0,
    Owner = 0x1,
}

public sealed record RoleAccessBlock
{
    public BlockRoleGrant Grant { get; init; } = BlockRoleGrant.None;
    public string? Claim { get; init; }
    public required string PrincipalId { get; init; }

    public bool Equals(RoleAccessBlock? obj) => obj is RoleAccessBlock document &&
        Grant == document.Grant &&
        Claim.ToNullIfEmpty() == document.Claim.ToNullIfEmpty() &&
        PrincipalId == document.PrincipalId;

    public override int GetHashCode() => HashCode.Combine(Grant, Claim, PrincipalId);

    public static RoleAccessBlock Create(BlockRoleGrant grant, string principalId, string? claim = null) => new RoleAccessBlock
    {
        Grant = grant,
        PrincipalId = principalId,
        Claim = claim,
    };
}

public static class BlockRoleAccessValidator
{
    public static IValidator<RoleAccessBlock> Validator { get; } = new Validator<RoleAccessBlock>()
        .RuleFor(x => x.Grant).Must(x => x.IsEnumValid<BlockRoleGrant>(), _ => "Invalid block grant")
        .RuleFor(x => x.Claim).Must(x => x.IsEmpty() || IdPatterns.IsName(x), _ => "Invalid claim")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .Build();

    public static bool HasAccess(this RoleAccessBlock subject, string principalId, BlockRoleGrant grant) =>
        subject.Grant.HasFlag(grant) &&
        subject.PrincipalId == principalId.Assert(x => IdPatterns.IsPrincipalId(x), "Invalid principalId");
}
