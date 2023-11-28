using Orleans;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record SubscriptionModel
{
    // Id = "schema/$system/{name}"
    [Id(0)] public string SubscriptionId { get; init; } = null!;
    [Id(1)] public string Name { get; init; } = null!;
    [Id(2)] public string ContactName { get; init; } = null!;
    [Id(3)] public string Email { get; init; } = null!;
    [Id(4)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;


    public bool Equals(SubscriptionModel? obj) => obj is SubscriptionModel document &&
        SubscriptionId == document.SubscriptionId &&
        Name == document.Name &&
        ContactName == document.ContactName &&
        Email == document.Email &&
        CreatedDate == document.CreatedDate;

    public override int GetHashCode() => HashCode.Combine(SubscriptionId, Name, ContactName);

    public static IValidator<SubscriptionModel> Validator { get; } = new Validator<SubscriptionModel>()
        .RuleFor(x => x.SubscriptionId).ValidResourceId(ResourceType.System, "subscription")
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.ContactName).NotEmpty()
        .RuleFor(x => x.Email).ValidResourceId(ResourceType.Principal)
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