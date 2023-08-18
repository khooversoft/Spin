using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Data;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

[GenerateSerializer, Immutable]
public sealed record TenantModel
{
    private const string _version = nameof(TenantModel) + "-v1";

    // Id = "tenant/$system/{name}"
    // Name is normal a domain (domain.com)
    [Id(0)] public string TenantId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();
    [Id(3)] public string Name { get; init; } = null!;
    [Id(4)] public string SubscriptionId { get; init; } = null!;
    [Id(5)] public string ContactName { get; init; } = null!;
    [Id(6)] public string Email { get; init; } = null!;
    [Id(7)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(8)] public bool AccountEnabled { get; init; } = false;
    [Id(9)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(10)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(TenantModel? obj) => obj is TenantModel document &&
        TenantId == document.TenantId &&
        Version == document.Version &&
        GlobalId == document.GlobalId &&
        Name == document.Name &&
        SubscriptionId == document.SubscriptionId &&
        ContactName == document.ContactName &&
        Email == document.Email &&
        DataObjects.SequenceEqual(document.DataObjects) &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(TenantId, GlobalId, Name, ContactName);
}


public static class TenantRegisterValidator
{
    public static IValidator<TenantModel> Validator { get; } = new Validator<TenantModel>()
        .RuleFor(x => x.TenantId).ValidResourceId()
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.SubscriptionId).ValidResourceId()
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidPrincipalId()
        .RuleFor(x => x.DataObjects).NotNull()
        .RuleForEach(x => x.DataObjects).Validate(DataObjectValidator.Validator)
        .Build();

    public static Option Validate(this TenantModel subject) => Validator.Validate(subject).ToOptionStatus();
}