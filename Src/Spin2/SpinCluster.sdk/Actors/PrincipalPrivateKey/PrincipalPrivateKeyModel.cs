using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalPrivateKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalPrivateKeyModel
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public string Audience { get; init; } = null!;
    [Id(4)] public byte[] PrivateKey { get; init; } = Array.Empty<byte>();

    public bool Equals(PrincipalPrivateKeyModel? obj) => obj is PrincipalPrivateKeyModel document &&
        KeyId == document.KeyId &&
        OwnerId == document.OwnerId &&
        Name == document.Name &&
        Audience == document.Audience &&
        PrivateKey.SequenceEqual(document.PrivateKey);

    public override int GetHashCode() => HashCode.Combine(KeyId, OwnerId, Name, Audience);
}


public static class PrincipalPrivateKeyModelValidator
{
    public static IValidator<PrincipalPrivateKeyModel> Validator { get; } = new Validator<PrincipalPrivateKeyModel>()
        .RuleFor(x => x.KeyId).NotEmpty()
        .RuleFor(x => x.OwnerId).NotEmpty()
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

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(subject.PrivateKey, subject.KeyId, subject.OwnerId, subject.Audience);
        return signature;
    }
}
