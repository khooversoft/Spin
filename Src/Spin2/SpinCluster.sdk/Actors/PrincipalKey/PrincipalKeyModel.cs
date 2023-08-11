using System.Linq;
using SpinCluster.sdk.Actors.Storage;
using SpinCluster.sdk.Application;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalKeyModel
{
    // Id = "principal-key/tenant/{principalId}/{name}"
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public string Audience { get; init; } = null!;
    [Id(4)] public byte[] PublicKey { get; init; } = Array.Empty<byte>();
    [Id(5)] public bool PrivateKeyExist { get; init; }
    [Id(6)] public bool AccountEnabled { get; init; } = false;
    [Id(7)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(8)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(PrincipalKeyModel? obj) => obj is PrincipalKeyModel document &&
        KeyId == document.KeyId &&
        PrincipalId == document.PrincipalId &&
        Name == document.Name &&
        Audience == document.Audience &&
        PublicKey.SequenceEqual(document.PublicKey) &&
        PrivateKeyExist == document.PrivateKeyExist &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(KeyId, PrincipalId, Name, Audience, PrivateKeyExist);

    public static ObjectId CreateId(PrincipalId principalId) => $"{SpinConstants.Schema.PrincipalKey}/{principalId.Domain}/{principalId}";
}


public static class PrincipalKeyValidator
{
    public static IValidator<PrincipalKeyModel> Validator { get; } = new Validator<PrincipalKeyModel>()
        .RuleFor(x => x.KeyId).ValidObjectId().Must(x => ((ObjectId)x).Paths.Count >= 1, x => $"Missing key name, KeyId={x}")
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.Audience).NotEmpty()
        .RuleFor(x => x.PublicKey).NotNull()
        .Build();

    public static ValidatorResult Validate(this PrincipalKeyModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static Option<PrincipalSignature> ToPrincipalSignature(this PrincipalKeyModel subject, ScopeContext context)
    {
        var validationResult = subject.Validate(context.Location());
        if (!validationResult.IsValid) validationResult.ToOption<PrincipalSignature>();

        var signature = PrincipalSignature.CreateFromPublicKeyOnly(subject.PublicKey, subject.KeyId, subject.PrincipalId, subject.Audience);
        return signature;
    }
}
