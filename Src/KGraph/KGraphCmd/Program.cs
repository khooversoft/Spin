using System.Reflection;
using KGraphCmd.Commands;
using Microsoft.Extensions.Configuration;
using Toolbox.CommandRouter;

Console.WriteLine($"KGraphCmd CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var state = await new CommandRouterBuilder()
    .SetArgs(args)
    .ConfigureAppConfiguration((config, service) =>
    {
        config.AddJsonFile("appsettings.json");
    })
    .AddCommand<GraphDb>()
    .AddCommand<TraceLog>()
    .AddCommand<TransactionLog>()
    .ConfigureService(x =>
    {
        //x.AddSpinClusterClients(LogLevel.Warning);
        //x.AddSpinClusterAdminClients(LogLevel.Warning);
        //x.AddSoftBankClients(LogLevel.Warning);
    })
    .Build()
    .Run();

return state;