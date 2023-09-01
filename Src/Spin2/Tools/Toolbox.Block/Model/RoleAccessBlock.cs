using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
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
