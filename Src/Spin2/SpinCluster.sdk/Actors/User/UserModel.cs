using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

[GenerateSerializer, Immutable]
public sealed record UserModel
{
    private const string _version = nameof(UserModel) + "-v1";

    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();
    [Id(3)] public string Email { get; init; } = null!;
    [Id(4)] public string DisplayName { get; init; } = null!;
    [Id(5)] public string FirstName { get; init; } = null!;
    [Id(6)] public string LastName { get; init; } = null!;
    [Id(7)] public IReadOnlyList<UserPhoneModel> Phone { get; init; } = Array.Empty<UserPhoneModel>();
    [Id(8)] public IReadOnlyList<UserAddressModel> Address { get; init; } = Array.Empty<UserAddressModel>();
    [Id(9)] public bool AccountEnabled { get; init; } = false;
    [Id(10)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(11)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(UserModel? obj) => obj is UserModel document &&
        UserId == document.UserId &&
        Version == document.Version &&
        GlobalId == document.GlobalId &&
        DisplayName == document.DisplayName &&
        FirstName == document.FirstName &&
        LastName == document.LastName &&
        Email == document.Email &&
        Phone.SequenceEqual(document.Phone) &&
        Address.SequenceEqual(document.Address) &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(UserId, GlobalId, DisplayName, DisplayName);

    public static ObjectId CreateId(PrincipalId userEmail) => $"{SpinConstants.Schema.User}/{userEmail.Domain}/{userEmail}";
}


public static class UserModelValidator
{
    public static IValidator<UserModel> Validator { get; } = new Validator<UserModel>()
        .RuleFor(x => x.UserId).ValidObjectId()
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .RuleFor(x => x.Email).ValidPrincipalId()
        .RuleFor(x => x.Phone).NotNull()
        .RuleForEach(x => x.Phone).Validate(UserPhoneModelValidator.Validator)
        .RuleFor(x => x.Address).NotNull()
        .RuleForEach(x => x.Address).Validate(UserAddressModelValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this UserModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}
