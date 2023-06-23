using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Tools;
using System.Security.Cryptography;
using Toolbox.Types;

namespace SpinCluster.sdk.Directory;

[GenerateSerializer, Immutable]
public record PrincipalKey
{
    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public byte[] PublicKey { get; init; } = null!;
    [Id(2)] public byte[]? PrivateKey { get; init; }

    public static PrincipalKey Create(ObjectId userId)
    {
        userId.NotNull();

        RSA rsa = RSA.Create();

        return new PrincipalKey
        {
            UserId = userId,
            PublicKey = rsa.ExportRSAPublicKey(),
            PrivateKey = rsa.ExportRSAPrivateKey(),
        };
    }
}


public static class PrincipalKeyValidator
{
    public static Validator<PrincipalKey> Validator { get; } = new Validator<PrincipalKey>()
        .RuleFor(x => x.UserId).NotEmpty()
        .RuleFor(x => x.PublicKey).NotNull()
        .Build();

    public static bool IsValid(this PrincipalKey subject) => Validator.Validate(subject).IsValid;
}