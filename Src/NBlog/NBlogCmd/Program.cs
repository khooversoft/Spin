using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBlogCmd.Activities;
using NBlogCmd.Application;
using Toolbox.Azure.DataLake;
using Toolbox.CommandRouter;
using Toolbox.Extensions;

Console.WriteLine($"NBlog Command - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();


UserSecretName option = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build()
    .Bind<UserSecretName>();

var state = await new CommandRouterBuilder()
    .SetArgs(args)
    .ConfigureAppConfiguration((config, service) =>
    {
        config.AddJsonFile("appsettings.json");
        if (option.UserSecrets.IsNotEmpty()) config.AddUserSecrets(option.UserSecrets);
        config.AddEnvironmentVariables("SPIN_CLI_");

        var cmdOption = config.Build().Bind<CmdOption>();
        service.AddSingleton(cmdOption);
        service.AddSingleton(cmdOption.Storage);
    })
    .AddCommand<Build>()
    .ConfigureService(x =>
    {
        x.AddSingleton<IDatalakeStore, DatalakeStore>();
    })
    .Build()
    .Run();

return state;