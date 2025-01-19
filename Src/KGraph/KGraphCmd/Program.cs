using System.Reflection;
using KGraphCmd.Application;
using KGraphCmd.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.CommandRouter;

Console.WriteLine($"KGraphCmd CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var state = await new CommandRouterBuilder()
    .SetArgs(args)
    .ConfigureAppConfiguration((config, service) =>
    {
        config.AddJsonFile("appsettings.json");
    })
    .AddCommand<Command>()
    .AddCommand<GraphDb>()
    .AddCommand<TraceLog>()
    .AddCommand<TransactionLog>()
    .ConfigureService(x =>
    {
        x.AddSingleton<GraphHostManager>();
        //x.AddSpinClusterClients(LogLevel.Warning);
        //x.AddSpinClusterAdminClients(LogLevel.Warning);
        //x.AddSoftBankClients(LogLevel.Warning);
    })
    .Build()
    .Run();

return state;