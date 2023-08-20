using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public sealed record UserCreateModel
{
    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string PrincipalId { get; init; } = null!;  // Email
    [Id(2)] public string DisplayName { get; init; } = null!;
    [Id(3)] public string FirstName { get; init; } = null!;
    [Id(4)] public string LastName { get; init; } = null!;

    public bool Equals(UserModel? obj) => obj is UserModel document &&
        UserId == document.UserId &&
        PrincipalId == document.PrincipalId &&
        DisplayName == document.DisplayName &&
        FirstName == document.FirstName &&
        LastName == document.LastName;

    public override int GetHashCode() => HashCode.Combine(UserId, PrincipalId, DisplayName, FirstName, LastName);
}


public static class UserCreateModelValidator
{
    public static IValidator<UserCreateModel> Validator { get; } = new Validator<UserCreateModel>()
        .RuleFor(x => x.UserId).ValidResourceId()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .Build();

    public static Option Validate(this UserCreateModel model) => Validator.Validate(model).ToOptionStatus();
}
