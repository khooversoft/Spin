using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

[GenerateSerializer, Immutable]
public record PrincipalKeyRequest
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
}


public static class PrincipalKeyRequestValidator
{
    public static IValidator<PrincipalKeyRequest> Validator { get; } = new Validator<PrincipalKeyRequest>()
        .RuleFor(x => x.KeyId).NotEmpty()
        .RuleFor(x => x.OwnerId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this PrincipalKeyRequest subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}
