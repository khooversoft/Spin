using FluentValidation;
using Toolbox.Azure.Identity;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Validation.Validators;

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
    //public static Validator<ObjectStoreOption> _validator = new Validator<ObjectStoreOption>()
    //    .AddRule(x => x.ClientIdentity).Validate(ClientSecretOptionValidator.Validator)
    //    .AddRule(x => x.DomainProfiles).NotEmpty()
    //    .AddRule(x => x.ContainerName).NotEmpty()
    //    .Build();

    //public static ValidatorResult<DomainProfileOption> Validate(this DomainProfileOption subject) => _validator.Validate(subject);
}

//public class ObjectStoreOptionValidator : AbstractValidator<ObjectStoreOption>
//{
//    public static ObjectStoreOptionValidator Default { get; } = new ObjectStoreOptionValidator();

//    public ObjectStoreOptionValidator()
//    {
//        RuleFor(x => x.DirectoryId).NotEmpty();
//        RuleFor(x => x.Subject).NotEmpty();
//        RuleFor(x => x.Version).NotEmpty();
//        RuleFor(x => x.PublicKey).NotNull();
//    }
//}

//public class DomainProfileValidator : AbstractValidator<DomainProfileOption>
//{
//    public static DomainProfileValidator Default { get; } = new DomainProfileValidator();

//    public DomainProfileValidator()
//    {
//        RuleFor(x => x.DirectoryId).NotEmpty();
//        RuleFor(x => x.Subject).NotEmpty();
//        RuleFor(x => x.Version).NotEmpty();
//        RuleFor(x => x.PublicKey).NotNull();
//    }
//}


//public static class ObjectStoreOptionExtensions
//{
//    public static void Verify(this IdentityEntry subject) => IdentityEntryValidator.Default.ValidateAndThrow(subject);

//    public static bool IsVerify(this IdentityEntry subject) => IdentityEntryValidator.Default.Validate(subject).IsValid;

//    public static IReadOnlyList<string> GetVerifyErrors(this IdentityEntry subject) => IdentityEntryValidator.Default
//        .Validate(subject)
//        .Errors
//        .Select(x => x.ErrorMessage)
//        .ToArray();
//}