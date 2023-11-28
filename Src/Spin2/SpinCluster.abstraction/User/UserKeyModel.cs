using Orleans;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.abstraction;

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

    public static IValidator<UserKeyModel> Validator { get; } = new Validator<UserKeyModel>()
        .RuleFor(x => x.KeyId).ValidResourceId(ResourceType.Owned, "kid")
        .RuleFor(x => x.PublicKeyId).ValidResourceId(ResourceType.Owned, "kid")
        .RuleFor(x => x.PrivateKeyId).ValidResourceId(ResourceType.Owned, "kid")
        .Build();
}

public static class UserKeyModelValidator
{
    public static Option Validate(this UserKeyModel subject) => UserKeyModel.Validator.Validate(subject).ToOptionStatus();
}
