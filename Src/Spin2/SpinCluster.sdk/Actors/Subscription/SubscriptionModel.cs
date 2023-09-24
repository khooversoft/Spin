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
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public string ContactName { get; init; } = null!;
    [Id(4)] public string Email { get; init; } = null!;
    [Id(5)] public IReadOnlyList<string> Tenants { get; init; } = Array.Empty<string>();
    [Id(6)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;


    public bool Equals(SubscriptionModel? obj) => obj is SubscriptionModel document &&
        SubscriptionId == document.SubscriptionId &&
        Version == document.Version &&
        Name == document.Name &&
        ContactName == document.ContactName &&
        Email == document.Email &&
        Tenants.SequenceEqual(document.Tenants) &&
        CreatedDate == document.CreatedDate;

    public override int GetHashCode() => HashCode.Combine(SubscriptionId, Name, ContactName);

    public static IValidator<SubscriptionModel> Validator { get; } = new Validator<SubscriptionModel>()
        .RuleFor(x => x.SubscriptionId).ValidResourceId(ResourceType.System, "subscription")
        .RuleFor(x => x.Version).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.Tenants).NotNull()
        .Build();
}


public static class SubscriptionModelValidator
{
    public static Option Validate(this SubscriptionModel subject) => SubscriptionModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SubscriptionModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}