using FluentValidation;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Validation.Validators;

namespace ObjectStore.sdk.Application;

public record DomainProfileOption
{
    public string DomainName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
}

public class DomainProfileOptionValidator : AbstractValidator<DomainProfileOption>
{
    public static DomainProfileOptionValidator Default { get; } = new DomainProfileOptionValidator();

    public DomainProfileOptionValidator()
    {
        RuleFor(x => x.DomainName).NotEmpty();
        RuleFor(x => x.AccountName).NotEmpty();
        RuleFor(x => x.ContainerName).NotEmpty();
    }
}

public static class DomainProfileOptionExtensions
{
    public static Validator<DomainProfileOption> _validator = new Validator<DomainProfileOption>()
        .RuleFor(x => x.DomainName).NotEmpty()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .Build();

    public static ValidatorResult<DomainProfileOption> Validate(this DomainProfileOption subject) => _validator.Validate(subject);
}
