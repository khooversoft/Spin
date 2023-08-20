﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public sealed record UserKeyModel
{
    [Id(0)] public string KeyId { get; init; } = null!;
    [Id(1)] public string PublicKeyId { get; init; } = null!;
    [Id(2)] public string PrivateKeyId { get; init; } = null!;

    public bool Equals(UserKeyModel? obj) => obj is UserKeyModel document &&
        PublicKeyId == document.PublicKeyId &&
        PrivateKeyId == document.PrivateKeyId;

    public override int GetHashCode() => HashCode.Combine(KeyId, PublicKeyId, PrivateKeyId);

    public static UserKeyModel Create(string principalId)
    {
        const string sign = "sign";

        principalId.NotEmpty();

        ResourceId resourceId = ResourceId.Create(principalId).ThrowOnError().Return();
        resourceId.GetKeyId().Assert(x => IdPatterns.IsPrincipalId(x), "Invalid KeyId");

        var userKeyModel = new UserKeyModel
        {
            KeyId = IdTool.CreateKid(principalId, sign),
            PublicKeyId = IdTool.CreatePublicKeyId(principalId, sign),
            PrivateKeyId = IdTool.CreatePrivateKeyId(principalId, sign),
        };

        return userKeyModel;
    }
}

public static class UserKeyModelValidator
{
    public static IValidator<UserKeyModel> Validator { get; } = new Validator<UserKeyModel>()
        .RuleFor(x => x.KeyId).ValidKeyId()
        .RuleFor(x => x.PublicKeyId).ValidObjectId()
        .RuleFor(x => x.PrivateKeyId).ValidObjectId()
        .Build();
}
