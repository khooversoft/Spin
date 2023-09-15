﻿using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public sealed record UserModel
{
    private const string _version = nameof(UserModel) + "-v1";

    // Id = "user:{principalId}"
    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();
    [Id(3)] public string PrincipalId { get; init; } = null!;  // Email
    [Id(4)] public string DisplayName { get; init; } = null!;
    [Id(5)] public string FirstName { get; init; } = null!;
    [Id(6)] public string LastName { get; init; } = null!;
    [Id(8)] public bool AccountEnabled { get; init; } = false;
    [Id(9)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(10)] public UserKeyModel UserKey { get; init; } = null!;

    public bool IsActive => AccountEnabled;

    public bool Equals(UserModel? obj) => obj is UserModel document &&
        UserId == document.UserId &&
        Version == document.Version &&
        GlobalId == document.GlobalId &&
        PrincipalId == document.PrincipalId &&
        DisplayName == document.DisplayName &&
        FirstName == document.FirstName &&
        LastName == document.LastName &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        UserKey == document.UserKey;

    public override int GetHashCode() => HashCode.Combine(UserId, GlobalId, DisplayName, DisplayName);
}


public static class UserModelValidator
{
    public static IValidator<UserModel> Validator { get; } = new Validator<UserModel>()
        .RuleFor(x => x.UserId).ValidResourceId(ResourceType.Owned, "user")
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .RuleFor(x => x.UserKey).Validate(UserKeyModel.Validator)
        .Build();

    public static Option Validate(this UserModel subject) => Validator.Validate(subject).ToOptionStatus();
}
