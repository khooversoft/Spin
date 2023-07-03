using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

[GenerateSerializer, Immutable]
public record PrincipalPublicKeyModel
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public byte[] PublicKey { get; init; } = null!;
}


public static class PrincipalPublicKeyValidator
{
    public static Validator<PrincipalPublicKeyModel> Validator { get; } = new Validator<PrincipalPublicKeyModel>()
        .RuleFor(x => x.KeyId).NotEmpty()
        .RuleFor(x => x.OwnerId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.PublicKey).NotNull()
        .Build();

    public static ValidatorResult Validate(this PrincipalPublicKeyModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}
