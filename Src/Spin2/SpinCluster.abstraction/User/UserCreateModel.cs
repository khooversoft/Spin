using Orleans;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.abstraction;

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

    public static IValidator<UserCreateModel> Validator { get; } = new Validator<UserCreateModel>()
        .RuleFor(x => x.UserId).ValidResourceId(ResourceType.Owned, "user")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .RuleForObject(x => x).Must(x =>
        {
            var userIdOption = ResourceId.Create(x.UserId);
            if (userIdOption.IsError()) return false;

            string userPrincipalId = userIdOption.Return().PrincipalId.NotEmpty();
            bool pass = x.PrincipalId.EqualsIgnoreCase(userPrincipalId);
            return pass;

        }, _ => "User id does not match principalId")
        .Build();
}


public static class UserCreateModelValidator
{
    public static Option Validate(this UserCreateModel model) => UserCreateModel.Validator.Validate(model).ToOptionStatus();

    public static bool Validate(this UserCreateModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
