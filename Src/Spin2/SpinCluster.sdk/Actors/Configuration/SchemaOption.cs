using SpinCluster.sdk.Actors.User;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Configuration;

[GenerateSerializer, Immutable]
public record SchemaOption
{
    [Id(0)] public string SchemaName { get; init; } = null!;
    [Id(1)] public string AccountName { get; init; } = null!;
    [Id(2)] public string ContainerName { get; init; } = null!;
    [Id(3)] public string? BasePath { get; init; }
}


public static class SchemaOptionValidator
{
    public static Validator<SchemaOption> Validator { get; } = new Validator<SchemaOption>()
        .RuleFor(x => x.SchemaName).NotEmpty()
        .RuleFor(x => x.AccountName).NotEmpty()
        .RuleFor(x => x.ContainerName).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this SchemaOption subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);
}