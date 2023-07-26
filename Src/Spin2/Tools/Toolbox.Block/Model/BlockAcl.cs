using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public sealed record BlockAcl
{
    public static string BlockType { get; } = "acl";

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

    public static ValidatorResult Validate(this BlockAcl subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static BlockAcl Verify(this BlockAcl subject, ScopeContextLocation location)
    {
        Validator.Validate(subject, location).ThrowOnError();
        return subject;
    }

    public static bool HasAccess(this BlockAcl subject, BlockAccessRequest request, NameId blockType, PrincipalId principalId)
    {
        subject.NotNull();
        principalId.NotNull();

        return subject.Items.Any(x => request.HasAccess(x) && x.BlockType == blockType.ToString() && x.PrincipalId == principalId);
    }

    public static bool HasWriteAccess(this BlockAcl subject, NameId blockType, PrincipalId principalId)
    {
        subject.NotNull();
        principalId.NotNull();

        return subject.Items.Any(x => x.WriteGrant && x.BlockType == blockType.ToString() && x.PrincipalId == principalId);
    }
}
