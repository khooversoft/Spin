using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Actors.User;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

[GenerateSerializer, Immutable]
public record TenantModel
{
    private const string _version = nameof(TenantModel) + "-v1";

    [Id(0)] public string TenantId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();
    [Id(3)] public string TenantName { get; init; } = null!;
    [Id(4)] public string Contact { get; init; } = null!;
    [Id(5)] public string Email { get; init; } = null!;
    [Id(6)] public IReadOnlyList<UserPhoneModel> Phone { get; init; } = Array.Empty<UserPhoneModel>();
    [Id(7)] public IReadOnlyList<UserAddressModel> Addresses { get; init; } = Array.Empty<UserAddressModel>();

    [Id(8)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(9)] public bool AccountEnabled { get; init; } = false;
    [Id(10)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(11)] public DateTime? ActiveDate { get; init; }
}


public static class TenantRegisterValidator
{
    public static IValidator<TenantModel> Validator { get; } = new Validator<TenantModel>()
        .RuleFor(x => x.TenantId).NotEmpty().ValidName()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.Contact).NotEmpty()
        .RuleFor(x => x.Email).NotEmpty()
        .RuleFor(x => x.Phone).NotNull().Must(x => x.Count > 0, _ => "A phone is required")
        .RuleFor(x => x.Addresses).NotNull().Must(x => x.Count > 0, _ => "A address is required")
        .RuleFor(x => x.DataObjects).NotNull()
        .Build();

    public static ValidatorResult Validate(this TenantModel subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}