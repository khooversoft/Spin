using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public sealed record AclBlock
{
    public static string BlockType { get; } = typeof(AclBlock).GetTypeName();

    public AclBlock() { }
    public AclBlock(IEnumerable<AccessBlock> access) => AccessRights = access.NotNull().ToArray();

    public IReadOnlyList<AccessBlock> AccessRights { get; init; } = Array.Empty<AccessBlock>();
    public IReadOnlyList<RoleAccessBlock> RoleAccess { get; init; } = Array.Empty<RoleAccessBlock>();
    public bool Equals(AclBlock? obj) => obj is AclBlock document &&
        AccessRights.SequenceEqual(document.AccessRights) &&
        RoleAccess.SequenceEqual(document.RoleAccess);
    public override int GetHashCode() => HashCode.Combine(AccessRights);
}


public static class AclBlockValidator
{
    public static IValidator<AclBlock> Validator { get; } = new Validator<AclBlock>()
        .RuleForEach(x => x.AccessRights).NotNull().Validate(AccessBlock.Validator)
        .Build();

    public static Option Validate(this AclBlock subject) => Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AclBlock subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static Option HasAccess(this AclBlock subject, string principalId, BlockGrant grant, string blockType) => subject
        .NotNull()
        .AccessRights.Any(x => x.HasAccess(principalId, grant, blockType))
        .ToOptionStatus(StatusCode.NotFound);

    public static Option HasAccess(this AclBlock subject, string principalId, BlockRoleGrant grant) => subject
        .NotNull()
        .RoleAccess.Any(x => x.HasAccess(principalId, grant))
        .ToOptionStatus(StatusCode.NotFound);
}
