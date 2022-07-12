using Contract.sdk.Client;
using ContractHost.sdk.Event;
using ContractHost.sdk.Model;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ContractHost.sdk.Host;

public interface IContractHostBuilder
{
    IContractHostBuilder AddCommand(string[] args);
    Task<IContractHost> Build();
    IContractHostBuilder ConfigureEvent(Action<IRouter<string, Task>> config);
    IContractHostBuilder ConfigureService(Action<IServiceCollection> config);
}


public class ContractHostBuilder : IContractHostBuilder
{
    private string[]? _args;
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private List<Action<IServiceCollection>> _serviceConfig = new List<Action<IServiceCollection>>();
    private List<Action<IRouter<string, Task>>> _routerConfig = new List<Action<IRouter<string, Task>>>();

    public static IContractHostBuilder Create() => new ContractHostBuilder();

    public IContractHostBuilder AddCommand(string[] args) => this.Action(x => x._args = args);

    public IContractHostBuilder ConfigureService(Action<IServiceCollection> config) => this.Action(_ => _serviceConfig.Add(config.NotNull()));
    public IContractHostBuilder ConfigureEvent(Action<IRouter<string, Task>> config) => this.Action(_ => _routerConfig.Add(config.NotNull()));

    public async Task<IContractHost> Build()
    {
        _args.NotNull(name: "Args are required, use AddCommand()");
        _routerConfig.Assert(x => x.Count > 0, "Events are required, use AddEvent<T>(...) or ConfigureEvent(...)");

        ContractHostOption contractHostOption = await BuildOption(_args);

        _serviceCollection.AddSingleton(contractHostOption);
        _serviceCollection.AddSingleton(new ContractContext(contractHostOption, _args));
        _serviceCollection.AddSingleton<ContractHost>();
        _serviceCollection.AddSingleton<IRouter<string, Task>, Router<string, Task>>();

        _serviceCollection.AddHttpClient<ContractClient>((service, httpClient) =>
        {
            ContractHostOption option = service.GetRequiredService<ContractHostOption>();
            httpClient.BaseAddress = new Uri(option.ContractUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ContractApiKey);
        });

        _serviceConfig.ForEach(x => x(_serviceCollection));

        _serviceCollection.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.AddDebug();
            configure.AddFilter(x => true);
        });

        IServiceProvider serviceProvider = _serviceCollection.BuildServiceProvider();

        IRouter<string, Task> router = serviceProvider.GetRequiredService<IRouter<string, Task>>();
        _routerConfig.ForEach(x => x(router));

        return serviceProvider.GetRequiredService<ContractHost>();
    }

    private static async Task<ContractHostOption> BuildOption(string[] args)
    {
        args.NotNull();

        string configFile = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build()
            .Bind<ContractHostOption>()
            .Func(x => x.ConfigFile.NotEmpty(name: "ConfigFile is required"))
            .Assert(x => File.Exists(x), x => $"File {x} does not exist");

        ContractHostOption option = new ConfigurationBuilder()
            .AddJsonFile(configFile)
            .AddCommandLine(args)
            .Build()
            .Bind<ContractHostOption>()
            .VerifyFull();

        return await DirectoryTools.Run(option.DirectoryUrl, option.DirectoryApiKey, async client =>
        {
            ServiceRecord contractRecord = await client.GetServiceRecord(option.RunEnvironment, "Contract");

            option = option with
            {
                ContractUrl = contractRecord.HostUrl,
                ContractApiKey = contractRecord.ApiKey,
            };

            return option.Verify();
        });
    }
}
