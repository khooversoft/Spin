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
    [Id(3)] public string Domain { get; init; } = null!;
    [Id(4)] public string SubscriptionId { get; init; } = null!;
    [Id(5)] public string ContactName { get; init; } = null!;
    [Id(6)] public string Email { get; init; } = null!;
    [Id(8)] public bool AccountEnabled { get; init; } = false;
    [Id(9)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(10)] public DateTime? ActiveDate { get; init; }

    public bool IsActive => AccountEnabled && ActiveDate != null;

    public bool Equals(TenantModel? obj) => obj is TenantModel document &&
        TenantId == document.TenantId &&
        Version == document.Version &&
        GlobalId == document.GlobalId &&
        Domain == document.Domain &&
        SubscriptionId == document.SubscriptionId &&
        ContactName == document.ContactName &&
        Email == document.Email &&
        AccountEnabled == document.AccountEnabled &&
        CreatedDate == document.CreatedDate &&
        ActiveDate == document.ActiveDate;

    public override int GetHashCode() => HashCode.Combine(TenantId, GlobalId, Domain, ContactName);

    public static IValidator<TenantModel> Validator { get; } = new Validator<TenantModel>()
        .RuleFor(x => x.TenantId).ValidResourceId(ResourceType.Tenant)
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.GlobalId).NotEmpty()
        .RuleFor(x => x.Domain).Must(x => IdPatterns.IsDomain(x), x => $"{x} not valid domain")
        .RuleFor(x => x.SubscriptionId).ValidResourceId(ResourceType.System, "subscription")
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidResourceId(ResourceType.Principal)
        .Build();
}


public static class TenantModelValidator
{
    public static Option Validate(this TenantModel subject) => TenantModel.Validator.Validate(subject).ToOptionStatus();
}