using Toolbox.Extensions;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Services;

public record SchemaOption
{
    public string SchemaName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
}


public static class SchemaOptionValidator
{
    public static Validator<SchemaOption> Validator { get; } = new Validator<SchemaOption>()
        .RuleFor(x => x.SchemaName).NotEmpty()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this SchemaOption subject) => Validator.Validate(subject);
    public static bool IsValid(this SchemaOption subject) => Validator.Validate(subject).IsValid;
}