using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public sealed record UserKeyModel
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string PublicKeyObjectId { get; init; } = null!;
    [Id(2)] public string PrivateKeyObjectId { get; init; } = null!;

    public bool Equals(UserKeyModel? obj) => obj is UserKeyModel document &&
        PublicKeyObjectId == document.PublicKeyObjectId &&
        PrivateKeyObjectId == document.PrivateKeyObjectId;

    public override int GetHashCode() => HashCode.Combine(KeyId, PublicKeyObjectId, PrivateKeyObjectId);

    public static UserKeyModel Create(PrincipalId principalId, string? name = null)
    {
        KeyId keyId = Toolbox.Types.KeyId.Create(principalId, name).Return();

        var userKeyModel = new UserKeyModel
        {
            KeyId = keyId,
            PublicKeyObjectId = IdTool.CreatePublicKeyObjectId(keyId),
            PrivateKeyObjectId = IdTool.CreatePrivateKeyObjectId(keyId),
        };

        return userKeyModel;
    }
}

public static class UserKeyModelValidator
{
    public static IValidator<UserKeyModel> Validator { get; } = new Validator<UserKeyModel>()
        .RuleFor(x => x.KeyId).ValidKeyId()
        .RuleFor(x => x.PublicKeyObjectId).ValidObjectId()
        .RuleFor(x => x.PrivateKeyObjectId).ValidObjectId()
        .Build();
}
