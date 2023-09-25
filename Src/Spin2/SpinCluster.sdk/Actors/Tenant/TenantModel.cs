using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

[GenerateSerializer, Immutable]
public sealed record TenantModel
{
    [Id(0)] public string TenantId { get; init; } = null!;
    [Id(1)] public string Domain { get; init; } = null!;
    [Id(2)] public string SubscriptionId { get; init; } = null!;
    [Id(3)] public string ContactName { get; init; } = null!;
    [Id(4)] public string Email { get; init; } = null!;
    [Id(5)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public bool Equals(TenantModel? obj) => obj is TenantModel document &&
        TenantId == document.TenantId &&
        Domain == document.Domain &&
        SubscriptionId == document.SubscriptionId &&
        ContactName == document.ContactName &&
        Email == document.Email &&
        CreatedDate == document.CreatedDate;

    public override int GetHashCode() => HashCode.Combine(TenantId, Domain, ContactName);

    public static IValidator<TenantModel> Validator { get; } = new Validator<TenantModel>()
        .RuleFor(x => x.TenantId).ValidResourceId(ResourceType.Tenant)
        .RuleFor(x => x.Domain).Must(x => IdPatterns.IsDomain(x), x => $"{x} not valid domain")
        .RuleFor(x => x.SubscriptionId).ValidResourceId(ResourceType.System, "subscription")
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidResourceId(ResourceType.Principal)
        .Build();
}


public static class TenantModelValidator
{
    public static Option Validate(this TenantModel subject) => TenantModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this TenantModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}