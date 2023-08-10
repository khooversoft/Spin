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

    // Id = "user/tenant/{principalId}"
    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();
    [Id(3)] public string PrincipalId { get; init; } = null!;
    [Id(4)] public string DisplayName { get; init; } = null!;
    [Id(5)] public string FirstName { get; init; } = null!;
    [Id(6)] public string LastName { get; init; } = null!;
    [Id(7)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(8)] public bool AccountEnabled { get; init; } = false;
    [Id(9)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(10)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(UserModel? obj) => obj is UserModel document &&
        UserId == document.UserId &&
        Version == document.Version &&
        GlobalId == document.GlobalId &&
        PrincipalId == document.PrincipalId &&
        DisplayName == document.DisplayName &&
        FirstName == document.FirstName &&
        LastName == document.LastName &&
        DataObjects.SequenceEqual(document.DataObjects) &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(UserId, GlobalId, DisplayName, DisplayName);

    public static ObjectId CreateId(PrincipalId principalId) => $"{SpinConstants.Schema.User}/{principalId.Domain}/{principalId}";
}


public static class UserModelValidator
{
    public static IValidator<UserModel> Validator { get; } = new Validator<UserModel>()
        .RuleFor(x => x.UserId).ValidObjectId()
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .RuleFor(x => x.DataObjects).NotNull()
        .RuleForEach(x => x.DataObjects).Validate(DataObjectValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this UserModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}
