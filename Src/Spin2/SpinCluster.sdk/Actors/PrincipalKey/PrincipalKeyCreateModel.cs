using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

[GenerateSerializer, Immutable]
public sealed record PrincipalKeyCreateModel
{
    // KeyId = "{principalId}/{name}"
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public string PrivateKeyObjectId { get; init; } = null!;
    [Id(4)] public bool AccountEnabled { get; init; } = false;

    public bool Equals(PrincipalKeyCreateModel? obj) => obj is PrincipalKeyCreateModel document &&
        KeyId == document.KeyId &&
        PrincipalId == document.PrincipalId &&
        Name == document.Name &&
        PrivateKeyObjectId == document.PrivateKeyObjectId &&
        AccountEnabled == document.AccountEnabled;

    public override int GetHashCode() => HashCode.Combine(KeyId, PrincipalId, Name);
}


public static class PrincipalKeyCreateModelValidator
{
    public static IValidator<PrincipalKeyCreateModel> Validator { get; } = new Validator<PrincipalKeyCreateModel>()
        .RuleFor(x => x.KeyId).ValidKeyId()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.PrivateKeyObjectId).ValidObjectId()
        .RuleForObject(x => x).Must(x => KeyId.Create(x.KeyId).Return().GetPrincipalId() == x.PrincipalId, _ => "PrincipalId does not match KeyId")
        .Build();
}

