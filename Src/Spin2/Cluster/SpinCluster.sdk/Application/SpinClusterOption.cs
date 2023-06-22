using SpinCluster.sdk.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Application;

public record SpinClusterOption
{
    public string StorageAccountConnectionString { get; init; } = null!;
    public string ApplicationInsightsConnectionString { get; init; } = null!;
    public ClientSecretOption ClientCredentials { get; init; } = null!;
    public IReadOnlyList<SchemaOption> Schemas { get; init; } = Array.Empty<SchemaOption>();
}

public record SchemaOption
{
    public string SchemaName { get; init; } = null!;
    public string AccountName { get; init; } = null!;
    public string ContainerName { get; init; } = null!;
    public string? BasePath { get; init; }
}


public static class SpinClusterOptionValidator
{
    public static Validator<SpinClusterOption> Validator { get; } = new Validator<SpinClusterOption>()
        .RuleFor(x => x.StorageAccountConnectionString).NotEmpty()
        .RuleFor(x => x.ApplicationInsightsConnectionString).NotEmpty()
        .RuleFor(x => x.ClientCredentials).Validate(ClientSecretOptionValidator.Validator)
        .RuleFor(x => x.Schemas).NotNull().Must(x => x.Count > 0, _ => "Schemas is empty")
        .RuleForEach(x => x.Schemas).Validate(SchemaOptionValidator.Validator)
        .Build();

    public static SpinClusterOption Verify(this SpinClusterOption option) => option.Action(x => Validator.Validate(option).ThrowOnError());
}

public static class SchemaOptionValidator
{
    public static Validator<SchemaOption> Validator { get; } = new Validator<SchemaOption>()
        .RuleFor(x => x.SchemaName).NotEmpty()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .Build();

    public static SchemaOption Verify(this SchemaOption option) => option.Action(x => Validator.Validate(option).ThrowOnError());
}

