﻿using SpinCluster.sdk.Application;
using SpinCluster.sdk.Services;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

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
        .RuleForEach(x => x.Schemas).Validate(SchemaOption.Validator)
        .RuleFor(x => x.Tenants).NotNull().Must(x => x.Count > 0, _ => "Tenants is empty")
        .RuleForEach(x => x.Tenants).NotEmpty()
        .Build();

    public static Option Validate(this SiloConfigOption subject) => Validator.Validate(subject).ToOptionStatus();
}
