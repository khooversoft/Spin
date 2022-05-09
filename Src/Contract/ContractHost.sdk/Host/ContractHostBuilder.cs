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

public class ContractHostBuilder : IContractHostBuilder
{
    private string[]? _args;
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private readonly List<EventClassRegistry> _eventRegistrations = new List<EventClassRegistry>();

    public static IContractHostBuilder Create() => new ContractHostBuilder();

    public IContractHostBuilder AddCommand(string[] args) => this.Action(x => x._args = args);

    public IContractHostBuilder AddSingleton<T>() where T : class => this.Action(x => x._serviceCollection.AddSingleton<T>());

    public IContractHostBuilder AddSingleton<T>(Func<IServiceProvider, T> implementationFactory) where T : class =>
        this.Action(x => x._serviceCollection.AddSingleton(service => implementationFactory(service)));

    public IContractHostBuilder AddEvent<T>() where T : class, IEventService
    {
        List<(MethodInfo methodInfo, EventNameAttribute attr)> methodsMap = typeof(T)
            .GetMethods()
            .Select(x => (methodInfo: x, attr: x.GetCustomAttribute<EventNameAttribute>()!))
            .Where(x => x.attr != null)
            .ToList()
            .Assert(x => x.Count > 0, "No methods marked with 'EventNameAttribute'");

        methodsMap
            .GroupBy(x => x.attr.EventName)
            .Where(x => x.Count() > 1)
            .Any()
            .Assert(x => x == false, "Multiple methods marked with the same 'EventNameAttribute'");

        methodsMap
            .Select(x =>
            {
                Delegate.CreateDelegate(typeof(EventNameHandler<T>), x.methodInfo)
                    .NotNull("Method signature is not valid");

                return new EventClassRegistry
                {
                    EventName = x.attr.EventName,
                    Type = typeof(T),
                    Method = (service, host, token) =>
                    {
                        MethodInfo methodInfo = x.methodInfo;
                        return (Task)methodInfo.Invoke(service, new object[] { host, token })!;
                    },
                };
            })
            .ForEach(x => _eventRegistrations.Add(x));

        return this;
    }

    public IContractHostBuilder AddEvent<T>(EventName contractEvent, EventNameHandler<T> method) where T : class, IEventService
    {
        contractEvent.Assert(x => x.IsValid(), $"Unknown eventName={(int)contractEvent}");
        method.NotNull(nameof(method));

        _serviceCollection.AddSingleton<T>();

        _eventRegistrations.Add(new EventClassRegistry
        {
            EventName = contractEvent,
            Type = typeof(T),
            Method = (service, host, token) => method((T)service, host, token)
        }.Verify());

        return this;
    }

    public async Task<IContractHost> Build()
    {
        _args.NotNull("Args are required, use AddCommand()");
        _eventRegistrations.Assert(x => x.Count > 0, "Events are required, use AddEvent<T>(...)");

        ContractHostOption contractHostOption = await BuildOption(_args);

        _serviceCollection.AddSingleton(contractHostOption);
        _serviceCollection.AddSingleton(new ContractContext(contractHostOption, _eventRegistrations, _args));
        _serviceCollection.AddSingleton<ContractHost>();

        _eventRegistrations.ForEach(x => _serviceCollection.AddSingleton(x.Type));

        _serviceCollection.AddHttpClient<ContractClient>((service, httpClient) =>
        {
            ContractHostOption option = service.GetRequiredService<ContractHostOption>();
            httpClient.BaseAddress = new Uri(option.ContractUrl);
            httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ContractApiKey);
        });

        _serviceCollection.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.AddDebug();
            configure.AddFilter(x => true);
        });

        return _serviceCollection
            .BuildServiceProvider()
            .GetRequiredService<ContractHost>();
    }

    private static async Task<ContractHostOption> BuildOption(string[] args)
    {
        args.NotNull(nameof(args));

        string configFile = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build()
            .Bind<ContractHostOption>()
            .Func(x => x.ConfigFile.NotEmpty("ConfigFile is required"))
            .Assert(x => File.Exists(x), x => $"File {x} does not exist");

        ContractHostOption option = new ConfigurationBuilder()
            .AddJsonFile(configFile)
            .AddCommandLine(args)
            .Build()
            .Bind<ContractHostOption>()
            .VerifyBootstrap();

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
