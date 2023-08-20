using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalKeyModel
{
    [Id(0)] public string PrincipalKeyId { get; init; } = null!;
    [Id(1)] public string KeyId { get; init; } = null!;
    [Id(2)] public string PrincipalId { get; init; } = null!;
    [Id(3)] public string Name { get; init; } = null!;
    [Id(4)] public string Audience { get; init; } = null!;
    [Id(5)] public byte[] PublicKey { get; init; } = Array.Empty<byte>();
    [Id(6)] public string PrincipalPrivateKeyId { get; init; } = null!;
    [Id(7)] public bool AccountEnabled { get; init; } = false;
    [Id(8)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(9)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(PrincipalKeyModel? obj) => obj is PrincipalKeyModel document &&
        PrincipalKeyId == document.PrincipalKeyId &&
        KeyId == document.KeyId &&
        PrincipalId == document.PrincipalId &&
        Name == document.Name &&
        Audience == document.Audience &&
        PublicKey.SequenceEqual(document.PublicKey) &&
        PrincipalPrivateKeyId == document.PrincipalPrivateKeyId &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(KeyId, KeyId, PrincipalId, Name, Audience, PrincipalPrivateKeyId);
}


public static class PrincipalKeyModelValidator
{
    public static IValidator<PrincipalKeyModel> Validator { get; } = new Validator<PrincipalKeyModel>()
        .RuleFor(x => x.PrincipalKeyId).ValidResourceId()
        .RuleFor(x => x.KeyId).ValidKeyId()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.Audience).NotEmpty()
        .RuleFor(x => x.PublicKey).NotNull()
        .RuleFor(x => x.PrincipalPrivateKeyId).ValidResourceId()
        .Build();

    public static Option Validate(this PrincipalKeyModel subject) => Validator.Validate(subject).ToOptionStatus();

    public static Option<PrincipalSignature> ToPrincipalSignature(this PrincipalKeyModel subject, ScopeContext context)
    {
        Option<IValidatorResult> validationResult = Validator.Validate(subject).LogResult(context.Location());
        if (validationResult.IsError()) return validationResult.ToOptionStatus<PrincipalSignature>();

        var signature = PrincipalSignature.CreateFromPublicKeyOnly(subject.PublicKey, subject.KeyId, subject.PrincipalId, subject.Audience);
        return signature;
    }
}
