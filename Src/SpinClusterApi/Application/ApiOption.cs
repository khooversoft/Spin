﻿using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Application;

public record ApiOption
{
    public string AppInsightsConnectionString { get; init; } = null!;
    public bool UseSwagger { get; init; }
    public string IpAddress { get; init; } = null!;
    public string? UserSecrets { get; init; }
}


public static class ApiOptionExtensions
{
    public static Validator<ApiOption> Validator { get; } = new Validator<ApiOption>()
        .RuleFor(x => x.AppInsightsConnectionString).NotEmpty()
        .RuleFor(x => x.IpAddress).NotEmpty()
        .Build();

    public static Option Validate(this ApiOption subject) => Validator.Validate(subject).ToOptionStatus();
}