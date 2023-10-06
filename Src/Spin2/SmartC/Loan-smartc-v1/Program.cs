﻿using System.CommandLine;
using System.Reflection;
using Loan_smartc_v1.Activitites;
using Loan_smartc_v1.Application;
using Loan_smartc_v1.Commands;
using LoanContract.sdk.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Trx;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Actors.Signature;
using Toolbox.Extensions;
using Toolbox.Tools;

try
{
    Console.WriteLine($"Loan-smartc-v1 CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine();

    (string[] ConfigArgs, string[] CommandLineArgs) = ArgumentTool.Split(args);

    AppOption option = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables("SPIN_SMARTC_")
        .AddCommandLine(ConfigArgs)
        .Build()
        .Bind<AppOption>()
        .Verify();

    using var serviceProvider = BuildContainer(option);

    int statusCode = await Run(serviceProvider, CommandLineArgs);
    Console.WriteLine($"StatusCode: {statusCode}");

    return statusCode;
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}

async Task<int> Run(IServiceProvider service, string[] args)
{
    try
    {
        AbortSignal abortSignal = service.GetRequiredService<AbortSignal>();
        abortSignal.StartTracking();

        var rc = new RootCommand()
        {
            service.GetRequiredService<RunCommand>(),
            service.GetRequiredService<CreateCommand>(),
        };

        await rc.InvokeAsync(args);
        return abortSignal.GetToken().IsCancellationRequested ? 1 : 0;

    }
    finally
    {
        Console.WriteLine();
        Console.WriteLine("Completed");
    }
}

ServiceProvider BuildContainer(AppOption option)
{
    var service = new ServiceCollection();
    service.AddLogging(config => config.AddConsole());
    service.AddSingleton(option);

    service.AddSingleton<RunCommand>();
    service.AddSingleton<CreateCommand>();

    service.AddSingleton<AbortSignal>();
    service.AddSingleton<CreateContract>();

    service.AddSingleton<LoanContractManager>();

    service.AddHttpClient<ContractClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<ScheduleWorkClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SignatureClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));
    service.AddHttpClient<SoftBankTrxClient>(client => client.BaseAddress = new Uri(option.ClusterApiUri));

    return service.BuildServiceProvider();
}

