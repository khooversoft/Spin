using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.User;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key.Private;

[GenerateSerializer, Immutable]
public record PrincipalPrivateKeyModel
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public string Audience { get; init; } = null!;
    [Id(4)] public byte[] PrivateKey { get; init; } = Array.Empty<byte>();
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
        PrincipalPrivateKeyModelValidator.Validate(subject, context.Location()).ThrowOnError();

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(subject.PrivateKey, subject.KeyId, subject.OwnerId, subject.Audience);
        return signature;
    }
}
