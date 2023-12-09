using LoanContract.sdk.Activities;
using LoanContract.sdk.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SpinClient.sdk;
using SpinCluster.abstraction;
using Toolbox.CommandRouter;
using Toolbox.Tools;

namespace LoanContract.sdk;

public static class LoanContractStartup
{
    public static CommandRouterBuilder CreateLocalAgent(params string[] args) => SpinClientHost.CreateLocalAgent()
        .SetArgs(args)
        .ConfigureAppConfiguration((config, service) =>
        {
            config.AddJsonFile("appsettings.json");
        });

    public static CommandRouterBuilder CreateSmartcWorkflow(ScheduleOption option, ClientOption clientOption, params string[] args) => SpinClientHost.CreateTestAgent(option)
        .SetArgs(args)
        .AddCommand<CreateContractActivity>()
        .AddCommand<PaymentActivity>()
        .AddCommand<InterestChargeActivity>()
        .ConfigureService(x =>
        {
            x.AddSingleton(clientOption.NotNull());
            x.AddSingleton<LoanContractManager>();
            x.AddSpinClusterClients(LogLevel.Warning);
            x.AddSoftBankClients(LogLevel.Warning);
        });
}
