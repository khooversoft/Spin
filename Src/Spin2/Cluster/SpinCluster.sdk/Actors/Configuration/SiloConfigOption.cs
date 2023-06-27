using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Services;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Configuration;

[GenerateSerializer, Immutable]
public record SiloConfigOption
{
    [Id(0)] public List<SchemaOption> Schemas { get; init; } = new List<SchemaOption>();
}


public static class SiloConfigOptionValidator
{
    public static Validator<SiloConfigOption> Validator { get; } = new Validator<SiloConfigOption>()
        .RuleFor(x => x.Schemas).NotNull().Must(x => x.Count > 0, _ => "Schemas is empty")
        .RuleForEach(x => x.Schemas).Validate(SchemaOptionValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this SiloConfigOption subject) => Validator.Validate(subject);
    public static bool IsValid(this SiloConfigOption subject) => Validator.Validate(subject).IsValid;
}
