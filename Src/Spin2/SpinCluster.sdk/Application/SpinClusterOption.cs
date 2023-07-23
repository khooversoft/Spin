﻿using Microsoft.Extensions.Configuration;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.Identity;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public record SpinClusterOption
{
    public string? ApplicationInsightsConnectionString { get; init; }
    public string BootConnectionString { get; init; } = null!;
    public ClientSecretOption Credentials { get; init; } = null!;
    public string? UserSecrets { get; init; }
}

public static class SpinClusterOptionValidator
{
    public static Validator<SpinClusterOption> Validator { get; } = new Validator<SpinClusterOption>()
        .RuleFor(x => x.BootConnectionString).Must(x => DatalakeLocation.ParseConnectionString(x).IsOk(), x => $"Connection string {x} is not valid")
        .RuleFor(x => x.Credentials).Validate(ClientSecretOptionValidator.Validator)
        .Build();

    public static ValidatorResult Validate(this SpinClusterOption subject) => Validator.Validate(subject);

    public static ValidatorResult Validate(this SpinClusterOption subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static SpinClusterOption Verify(this SpinClusterOption subject)
    {
        subject.Validate().Assert(x => x.IsValid, x => $"Option is not valid, errors={x.FormatErrors()}");
        return subject;
    }
}


public static class SpinClusterOptionTool
{
    public static SpinClusterOption Read(string appsettingFile = "appsettings.json")
    {
        SpinClusterOption option = new ConfigurationBuilder()
            .AddJsonFile(appsettingFile)
            .Build()
            .Bind<SpinClusterOption>();

        option = option.UserSecrets switch
        {
            null => option,

            string v => new ConfigurationBuilder()
                .AddJsonFile(appsettingFile)
                .AddUserSecrets(v)
                .Build()
                .Bind<SpinClusterOption>(),
        };

        return option.Verify();
    }
}