using Toolbox.Tools.Validation;

namespace ObjectStore.sdk.Application;

public record DomainProfileOption
{
    public string DomainName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
}

public static class DomainProfileOptionValidator
{
    public static Validator<DomainProfileOption> Validator { get; } = new Validator<DomainProfileOption>()
        .RuleFor(x => x.DomainName).NotEmpty()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this DomainProfileOption subject) => Validator.Validate(subject);
}
