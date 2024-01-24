using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public record StorageOption
{
    public DatalakeOption Storage { get; init; } = null!;
    public string DefaultDbName { get; init; } = null!;

    public static Validator<StorageOption> Validator { get; } = new Validator<StorageOption>()
        .RuleFor(x => x.Storage).Validate(DatalakeOption.Validator)
        .RuleFor(x => x.DefaultDbName).ValidName()
        .Build();
}

public static class StorageOptionExtensions
{
    public static Option Validate(this StorageOption subject) => StorageOption.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this StorageOption subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
