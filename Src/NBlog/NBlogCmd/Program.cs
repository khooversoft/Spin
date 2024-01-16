using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBlog.sdk;
using NBlogCmd.Activities;
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

        var cmdOption = config.Build().Bind<StorageOption>();
        service.AddSingleton(cmdOption);
        service.AddSingleton(cmdOption.Storage);
    })
    .AddCommand<Build>()
    .AddCommand<Upload>()
    //.AddCommand<BuildWordList>()
    .ConfigureService(x =>
    {
        x.AddSingleton<PackageBuild>();
        x.AddSingleton<PackageUpload>();
        //x.AddSingleton<BuildWordTokenList>();
    })
    .Build()
    .Run();

return state;