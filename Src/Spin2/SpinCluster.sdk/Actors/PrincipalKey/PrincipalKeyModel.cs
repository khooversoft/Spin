using System.Linq;
using SpinCluster.sdk.Actors.Storage;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalKeyModel
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public string Audience { get; init; } = null!;
    [Id(4)] public byte[] PublicKey { get; init; } = Array.Empty<byte>();
    [Id(5)] public bool PrivateKeyExist { get; init; }

    public bool Equals(PrincipalKeyModel? obj) => obj is PrincipalKeyModel document &&
        KeyId == document.KeyId &&
        OwnerId == document.OwnerId &&
        Name == document.Name &&
        Audience == document.Audience &&
        PublicKey.SequenceEqual(document.PublicKey) &&
        PrivateKeyExist == document.PrivateKeyExist;

    public override int GetHashCode() => HashCode.Combine(KeyId, OwnerId, Name, Audience, PrivateKeyExist);
}


public static class PrincipalKeyValidator
{
    public static IValidator<PrincipalKeyModel> Validator { get; } = new Validator<PrincipalKeyModel>()
        .RuleFor(x => x.KeyId).ValidObjectId()
        .RuleFor(x => x.OwnerId).ValidName()
        .RuleFor(x => x.Name).NotEmpty()
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

        var signature = PrincipalSignature.CreateFromPublicKeyOnly(subject.PublicKey, subject.KeyId, subject.OwnerId, subject.Audience);
        return signature;
    }
}
