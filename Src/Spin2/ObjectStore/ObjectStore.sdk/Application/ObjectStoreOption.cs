using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;

namespace ObjectStore.sdk.Application;

public record ObjectStoreOption
{
    public ClientSecretOption ClientIdentity { get; init; } = null!;
    public IReadOnlyList<DomainProfileOption> DomainProfiles { get; init; } = Array.Empty<DomainProfileOption>();
    public string AppicationInsightsConnectionString { get; init; } = null!;
    public string IpAddress { get; init; } = null!;
}


public static class ObjectStoreOptionValidator
{
    public static Validator<ObjectStoreOption> Validator = new Validator<ObjectStoreOption>()
        .RuleFor(x => x.ClientIdentity).Validate(ClientSecretOptionValidator.Validator)
        .RuleForEach(x => x.DomainProfiles).Validate(DomainProfileOptionValidator.Validator)
        .RuleFor(x => x.AppicationInsightsConnectionString).NotEmpty()
        .RuleFor(x => x.IpAddress).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this ObjectStoreOption subject) => Validator.Validate(subject);

    public static ObjectStoreOption Verify(this ObjectStoreOption subject) => subject.Action(x => Validator.Validate(x).ThrowOnError());
}
