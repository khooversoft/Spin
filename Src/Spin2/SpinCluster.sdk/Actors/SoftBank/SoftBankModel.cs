using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.SoftBank;
using SpinCluster.sdk.Actors.User;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.SoftBank;

[GenerateSerializer, Immutable]
public record SoftBankModel
{
    private const string _version = nameof(SoftBankModel) + "-v1";

    [Id(0)] public string UserId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();

    [Id(3)] public string DisplayName { get; init; } = null!;
    [Id(4)] public string FirstName { get; init; } = null!;
    [Id(5)] public string LastName { get; init; } = null!;
    [Id(6)] public string Email { get; init; } = null!;
    [Id(7)] public IReadOnlyList<UserPhoneModel> Phone { get; init; } = Array.Empty<UserPhoneModel>();
    [Id(8)] public IReadOnlyList<UserAddressModel> Addresses { get; init; } = Array.Empty<UserAddressModel>();

    [Id(9)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(10)] public bool AccountEnabled { get; init; } = false;
    [Id(11)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(12)] public DateTime? ActiveDate { get; init; }
}


public static class SoftBankModelValidator
{
    public static IValidator<UserModel> Validator { get; } = new Validator<UserModel>()
        .RuleFor(x => x.UserId).NotEmpty().ValidName()
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.DisplayName).NotEmpty()
        .RuleFor(x => x.FirstName).NotEmpty()
        .RuleFor(x => x.LastName).NotEmpty()
        .RuleFor(x => x.Email).NotEmpty()
        .RuleFor(x => x.Phone).NotNull()
        .RuleForEach(x => x.Phone).Validate(UserPhoneModelValidator.Validator)
        .RuleFor(x => x.Addresses).NotNull()
        .RuleForEach(x => x.Addresses).Validate(UserAddressModelValidator.Validator)
        .RuleForEach(x => x.DataObjects).NotNull()
        .RuleForEach(x => x.DataObjects).Validate(DataObjectValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this UserModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}
