using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Application;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalKeyCreateModel
{
    // Id = "principal-key/tenant/{principalId}/{name}"
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public bool AccountEnabled { get; init; } = false;

    public bool Equals(PrincipalKeyCreateModel? obj) => obj is PrincipalKeyCreateModel document &&
        KeyId == document.KeyId &&
        PrincipalId == document.PrincipalId &&
        Name == document.Name &&
        AccountEnabled == document.AccountEnabled;

    public override int GetHashCode() => HashCode.Combine(KeyId, PrincipalId, Name);

    public static ObjectId CreateId(PrincipalId principalId) => $"{SpinConstants.Schema.PrincipalKey}/{principalId.Domain}/{principalId}";
}


public static class PrincipalKeyCreateModelValidator
{
    public static IValidator<PrincipalKeyCreateModel> Validator { get; } = new Validator<PrincipalKeyCreateModel>()
        .RuleFor(x => x.KeyId).ValidObjectId().Must(x => ((ObjectId)x).Paths.Count >= 1, x => $"Missing key name, KeyId={x}")
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.Name).ValidName()
        .RuleForObject(x => x).Must(x => ObjectId.Create(x.KeyId).Return().Path == x.PrincipalId, _ => "PrincipalId does not match KeyId")
        .Build();

    public static ValidatorResult Validate(this PrincipalKeyCreateModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}

