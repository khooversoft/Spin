using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

[GenerateSerializer, Immutable]
public record PrincipalKeyModel
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public byte[] PublicKey { get; init; } = Array.Empty<byte>();
    [Id(4)] public bool PrivateKeyExist { get; init; }
}


public static class PrincipalKeyValidator
{
    public static IValidator<PrincipalKeyModel> Validator { get; } = new Validator<PrincipalKeyModel>()
        .RuleFor(x => x.KeyId).NotEmpty()
        .RuleFor(x => x.OwnerId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.PublicKey).NotNull()
        .Build();

    public static ValidatorResult Validate(this PrincipalKeyModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}
