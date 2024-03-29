﻿using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Application;

internal record CmdOption
{
    public string ClusterApiUri { get; init; } = null!;
    public string WorkingFolder { get; init; } = null!;
}


internal static class CmdOptionExtensions
{
    public static Validator<CmdOption> Validator { get; } = new Validator<CmdOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .RuleFor(x => x.WorkingFolder).NotEmpty()
        .Build();

    public static Option Validate(this CmdOption option) => Validator.Validate(option).ToOptionStatus();

    public static CmdOption Verify(this CmdOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}
