using System.Reflection;
using Loan_smartc_v1.Activitites;
using Loan_smartc_v1.Application;
using LoanContract.sdk.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SpinCluster.sdk.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;

Console.WriteLine($"Loan-smartc-v1 CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var state = await new CommandRouterBuilder()
    .SetArgs(args)
    .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("SPIN_SMARTC_"))
    .ConfigureAppConfiguration((config, service) => service.AddSingleton(config.Build().Bind<AppOption>().Verify()))
    .AddCommand<CreateContract>()
    .AddCommand<Payment>()
    .ConfigureService(x =>
    {
        x.AddLoanContract();
        x.AddSpinClusterClients(LogLevel.Warning);
        x.AddSoftBankClients(LogLevel.Warning);
    })
    .Build()
    .Run();

Console.WriteLine($"Return state: {state}");
return state;

