using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Services;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Configuration;

[GenerateSerializer, Immutable]
public record SiloConfigOption
{
    [Id(0)] public IReadOnlyList<SchemaOption> Schemas { get; init; } = Array.Empty<SchemaOption>();
    [Id(1)] public IReadOnlyList<string> Tenants { get; init; } = Array.Empty<string>();
}


public static class SiloConfigOptionValidator
{
    public static IValidator<SiloConfigOption> Validator { get; } = new Validator<SiloConfigOption>()
        .RuleFor(x => x.Schemas).NotNull().Must(x => x.Count > 0, _ => "Schemas is empty")
        .RuleForEach(x => x.Schemas).Validate(SchemaOptionValidator.Validator)
        .RuleFor(x => x.Tenants).NotNull().Must(x => x.Count > 0, _ => "Tenants is empty")
        .RuleForEach(x => x.Tenants).NotEmpty()
        .Build();

    public static Option Validate(this SiloConfigOption subject) => Validator.Validate(subject).ToOptionStatus();
}
