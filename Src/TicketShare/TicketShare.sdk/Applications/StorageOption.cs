using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record StorageOption
{
    public DatalakeOption Storage { get; init; } = null!;

    public static IValidator<StorageOption> Validator { get; } = new Validator<StorageOption>()
        .RuleFor(x => x.Storage).Validate(DatalakeOption.Validator)
        .Build();
}


public static class StorageOptionExtensions
{
    public static Option<IValidatorResult> Validate(this StorageOption subject) => StorageOption.Validator.Validate(subject);
}