using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public sealed record BlockAcl
{
    public static string BlockType { get; } = typeof(BlockAcl).GetTypeName();

    public BlockAcl() { }
    public BlockAcl(IEnumerable<BlockAccess> access) => AccessRights = access.NotNull().ToArray();

    public IReadOnlyList<BlockAccess> AccessRights { get; init; } = Array.Empty<BlockAccess>();

    public bool Equals(BlockAcl? obj)
    {
        return obj is BlockAcl document && AccessRights.SequenceEqual(document.AccessRights);
    }

    public override int GetHashCode() => HashCode.Combine(AccessRights);
}


public static class BlockAclValidator
{
    public static IValidator<BlockAcl> Validator { get; } = new Validator<BlockAcl>()
        .RuleForEach(x => x.AccessRights).NotNull().Validate(BlockAccessValidator.Validator)
        .Build();

    public static Option Validate(this BlockAcl subject) => Validator.Validate(subject).ToOptionStatus();

    public static Option HasAccess(this BlockAcl subject, string principalId, BlockGrant grant, string blockType) => subject
        .NotNull()
        .AccessRights.Any(x => x.HasAccess(principalId, grant, blockType))
        .ToOptionStatus();
    
    public static Option HasAccess(this BlockAcl subject, string principalId, BlockGrant grant) => subject
        .NotNull()
        .AccessRights.Any(x => x.HasAccess(principalId, grant))
        .ToOptionStatus();
}
