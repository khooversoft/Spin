using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlogCmd.Application;

internal record CmdOption
{
    public DatalakeOption Storage { get; init; } = null!;
}

internal static class CmdOptionExtensions
{
    public static Validator<CmdOption> Validator { get; } = new Validator<CmdOption>()
        .RuleFor(x => x.Storage).Validate(DatalakeOption.Validator)
        .Build();

    public static Option Validate(this CmdOption option) => Validator.Validate(option).ToOptionStatus();

    public static CmdOption Verify(this CmdOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}
