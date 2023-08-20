using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public sealed record BlockAcl
{
    public static string BlockType { get; } = "acl";

    public BlockAcl() { }
    public BlockAcl(IEnumerable<BlockAccess> access) => Items = access.NotNull().ToArray();

    public IReadOnlyList<BlockAccess> Items { get; init; } = Array.Empty<BlockAccess>();

    public bool Equals(BlockAcl? obj)
    {
        return obj is BlockAcl document && Items.SequenceEqual(document.Items);
    }

    public override int GetHashCode() => HashCode.Combine(Items);
}


public static class BlockAclValidator
{
    public static IValidator<BlockAcl> Validator { get; } = new Validator<BlockAcl>()
        .RuleForEach(x => x.Items).NotNull().Validate(BlockAccessValidator.Validator)
        .Build();

    public static Option Validate(this BlockAcl subject) => Validator.Validate(subject).ToOptionStatus();

    public static bool HasAccess(this BlockAcl subject, BlockGrant grant, string blockType, string principalId)
    {
        subject.NotNull();

        return subject.Items.Any(x => x.HasAccess(grant, blockType, principalId));
    }
}
