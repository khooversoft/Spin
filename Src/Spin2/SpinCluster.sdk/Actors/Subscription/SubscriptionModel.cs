using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Data;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Subscription;

[GenerateSerializer, Immutable]
public sealed record SubscriptionModel
{
    private const string _version = nameof(SubscriptionModel) + "-v1";

    // Id = "schema/$system/{name}"
    [Id(0)] public string SubscriptionId { get; init; } = null!;
    [Id(1)] public string Version { get; init; } = _version;
    [Id(2)] public string GlobalId { get; init; } = Guid.NewGuid().ToString();
    [Id(3)] public string Name { get; init; } = null!;
    [Id(4)] public string ContactName { get; init; } = null!;
    [Id(5)] public string Email { get; init; } = null!;
    [Id(6)] public IReadOnlyList<DataObject> DataObjects { get; init; } = Array.Empty<DataObject>();
    [Id(7)] public IReadOnlyList<string> Tenants { get; init; } = Array.Empty<string>();
    [Id(8)] public bool AccountEnabled { get; init; } = false;
    [Id(9)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(10)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(SubscriptionModel? obj) => obj is SubscriptionModel document &&
        SubscriptionId == document.SubscriptionId &&
        Version == document.Version &&
        GlobalId == document.GlobalId &&
        Name == document.Name &&
        ContactName == document.ContactName &&
        Email == document.Email &&
        DataObjects.SequenceEqual(document.DataObjects) &&
        Tenants.SequenceEqual(document.Tenants) &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(SubscriptionId, GlobalId, Name, ContactName);
}


public static class SubscriptionModelValidator
{
    public static IValidator<SubscriptionModel> Validator { get; } = new Validator<SubscriptionModel>()
        .RuleFor(x => x.SubscriptionId).ValidResourceId(ResourceType.System)
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.Name).ValidName()
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.DataObjects).NotNull()
        .RuleForEach(x => x.DataObjects).Validate(DataObjectValidator.Validator)
        .RuleFor(x => x.Tenants).NotNull()
        .Build();

    public static Option Validate(this SubscriptionModel subject) => Validator.Validate(subject).ToOptionStatus();
}