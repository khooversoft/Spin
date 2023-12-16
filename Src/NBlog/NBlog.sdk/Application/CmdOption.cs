using NBlog.sdk.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.Application;

public record CmdOption
{
    public DatalakeOption Storage { get; init; } = null!;
}

public static class CmdOptionExtensions
{
    public static Validator<CmdOption> Validator { get; } = new Validator<CmdOption>()
        .RuleFor(x => x.Storage).Validate(DatalakeOption.Validator)
        .Build();

    public static Option Validate(this CmdOption option) => Validator.Validate(option).ToOptionStatus();

    public static CmdOption Verify(this CmdOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}
