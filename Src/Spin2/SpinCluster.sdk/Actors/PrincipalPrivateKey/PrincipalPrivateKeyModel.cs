using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalPrivateKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalPrivateKeyModel
{
    [Id(0)] public string PrincipalPrivateKeyId { get; init; } = null!;
    [Id(1)] public string KeyId { get; init; } = null!;
    [Id(2)] public string PrincipalId { get; init; } = null!;
    [Id(3)] public string Name { get; init; } = null!;
    [Id(4)] public string Audience { get; init; } = null!;
    [Id(6)] public byte[] PrivateKey { get; init; } = Array.Empty<byte>();
    [Id(7)] public bool AccountEnabled { get; init; } = false;
    [Id(8)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(9)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(PrincipalPrivateKeyModel? obj) => obj is PrincipalPrivateKeyModel document &&
        PrincipalPrivateKeyId == document.PrincipalPrivateKeyId &&
        KeyId == document.KeyId &&
        PrincipalId == document.PrincipalId &&
        Name == document.Name &&
        Audience == document.Audience &&
        PrivateKey.SequenceEqual(document.PrivateKey) &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(PrincipalPrivateKeyId, PrincipalId, Name, Audience);
}


public static class PrincipalPrivateKeyModelValidator
{
    public static IValidator<PrincipalPrivateKeyModel> Validator { get; } = new Validator<PrincipalPrivateKeyModel>()
        .RuleFor(x => x.PrincipalPrivateKeyId).ValidResourceId(ResourceType.Owned)
        .RuleFor(x => x.KeyId).ValidResourceId(ResourceType.Owned, "kid")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Audience).NotEmpty()
        .RuleFor(x => x.PrivateKey).NotNull()
        .Build();

    public static Option Validate(this PrincipalPrivateKeyModel subject) => Validator.Validate(subject).ToOptionStatus();

    public static PrincipalSignature ToPrincipalSignature(this PrincipalPrivateKeyModel subject, ScopeContext context)
    {
        subject.Validate().LogResult(context.Location()).ThrowOnError();

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(subject.PrivateKey, subject.KeyId, subject.PrincipalId, subject.Audience);
        return signature;
    }
}
