﻿using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;

namespace SpinClusterCmd.Application;

internal record CmdOption
{
    public string ClusterApi { get; init; } = null!;
}


internal static class CmdOptionExtensions
{
    public static Validator<CmdOption> Validator { get; } = new Validator<CmdOption>()
        .RuleFor(x => x.ClusterApi).NotEmpty()
        .Build();

    public static CmdOption Verify(this CmdOption option) => option.Action(x => Validator.Validate(x).ThrowOnError());
}
