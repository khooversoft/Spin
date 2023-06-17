using Toolbox.Tools.Validation;

namespace ObjectStore.sdk.Application;

public record DomainProfile
{
    public string DomainName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
}

public static class DomainProfileValidator
{
    public static Validator<DomainProfile> Validator { get; } = new Validator<DomainProfile>()
        .RuleFor(x => x.DomainName).NotEmpty()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this DomainProfile subject) => Validator.Validate(subject);
}
