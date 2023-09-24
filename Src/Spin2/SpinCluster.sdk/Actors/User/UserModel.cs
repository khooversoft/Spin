using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public sealed record UserModel
{
    // Id = "user:{principalId}"
    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;  // {user}@{domain}
    [Id(2)] public string DisplayName { get; init; } = null!;
    [Id(3)] public string FirstName { get; init; } = null!;
    [Id(4)] public string LastName { get; init; } = null!;
    [Id(5)] public bool AccountEnabled { get; init; } = false;
    [Id(6)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(7)] public UserKeyModel UserKey { get; init; } = null!;

    public bool IsActive => AccountEnabled;

    public bool Equals(UserModel? obj) => obj is UserModel document &&
        UserId == document.UserId &&
        PrincipalId == document.PrincipalId &&
        DisplayName == document.DisplayName &&
        FirstName == document.FirstName &&
        LastName == document.LastName &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        UserKey == document.UserKey;

    public override int GetHashCode() => HashCode.Combine(UserId, DisplayName, DisplayName);

    public static IValidator<UserModel> Validator { get; } = new Validator<UserModel>()
        .RuleFor(x => x.UserId).ValidResourceId(ResourceType.Owned, "user")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .RuleFor(x => x.UserKey).Validate(UserKeyModel.Validator)
        .Build();
}


public static class UserModelValidator
{
    public static Option Validate(this UserModel subject) => UserModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this UserModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
