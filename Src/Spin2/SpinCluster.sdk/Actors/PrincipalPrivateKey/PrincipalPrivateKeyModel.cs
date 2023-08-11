using SpinCluster.sdk.Application;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalPrivateKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalPrivateKeyModel
{
    // Id = "principal-private-key/tenant/{principalId}/{name}"
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public string Audience { get; init; } = null!;
    [Id(4)] public byte[] PrivateKey { get; init; } = Array.Empty<byte>();
    [Id(5)] public bool AccountEnabled { get; init; } = false;
    [Id(6)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(7)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(PrincipalPrivateKeyModel? obj) => obj is PrincipalPrivateKeyModel document &&
        KeyId == document.KeyId &&
        PrincipalId == document.PrincipalId &&
        Name == document.Name &&
        Audience == document.Audience &&
        PrivateKey.SequenceEqual(document.PrivateKey) &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(KeyId, PrincipalId, Name, Audience);
    public static ObjectId CreateId(PrincipalId principalId) => $"{SpinConstants.Schema.PrincipalPrivateKey}/{principalId.Domain}/{principalId}";

}


public static class PrincipalPrivateKeyModelValidator
{
    public static IValidator<PrincipalPrivateKeyModel> Validator { get; } = new Validator<PrincipalPrivateKeyModel>()
        .RuleFor(x => x.KeyId).NotEmpty()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Audience).NotEmpty()
        .RuleFor(x => x.PrivateKey).NotNull()
        .Build();

    public static ValidatorResult Validate(this PrincipalPrivateKeyModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static PrincipalSignature ToPrincipalSignature(this PrincipalPrivateKeyModel subject, ScopeContext context)
    {
        subject.Validate(context.Location()).ThrowOnError();

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(subject.PrivateKey, subject.KeyId, subject.PrincipalId, subject.Audience);
        return signature;
    }
}
