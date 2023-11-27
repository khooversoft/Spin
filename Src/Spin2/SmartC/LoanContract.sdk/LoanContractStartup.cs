using LoanContract.sdk.Activities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SpinAgent.sdk;
using SpinCluster.sdk.Application;
using Toolbox.CommandRouter;
using Toolbox.Extensions;

namespace LoanContract.sdk;

public static class LoanContractStartup
{
    public static CommandRouterBuilder Create(params string[] args) => new CommandRouterBuilder()
        .SetArgs(args)
        .ConfigureAppConfiguration((config, service) =>
        {
            config.AddJsonFile("appsettings.json");
            config.AddEnvironmentVariables("SPIN_SMARTC_");
            service.AddSingleton(config.Build().Bind<AgentOption>().Verify());
        })
        .AddCommand<CreateContractActivity>()
        .AddCommand<PaymentActivity>()
        .AddCommand<InterestChargeActivity>()
        .ConfigureService(x =>
        {
            x.AddSingleton<IRunSmartc, RunSmartC>();
            x.AddSingleton<WorkMonitor>();
            x.AddSpinClusterClients(LogLevel.Warning);
            x.AddSoftBankClients(LogLevel.Warning);
            x.AddSpinAgent();

            //x.AddLoanContract();
        });

    public static CommandRouterBuilder CreateInMemory<T>(AgentOption agentOption, params string[] args) where T : class, IRunSmartc => new CommandRouterBuilder()
        .SetArgs(args)
        .ConfigureAppConfiguration((config, service) => service.AddSingleton(agentOption.Verify()))
        .AddCommand<CreateContractActivity>()
        .AddCommand<PaymentActivity>()
        .AddCommand<InterestChargeActivity>()
        .ConfigureService(x =>
        {
            x.AddSingleton<IRunSmartc, T>();
            x.AddSpinClusterClients(LogLevel.Warning);
            x.AddSoftBankClients(LogLevel.Warning);
        });
}
