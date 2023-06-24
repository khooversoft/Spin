using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Services;

public record SiloConfigOption
{
    public IReadOnlyList<SchemaOption> Schemas { get; init; } = Array.Empty<SchemaOption>();
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
