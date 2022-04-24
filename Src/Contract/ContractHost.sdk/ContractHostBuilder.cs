using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ContractHost.sdk;

public class ContractHostBuilder
{
    private string[]? _args;
    private readonly IServiceCollection _serviceCollection;

    public ContractHostBuilder()
    {
        _serviceCollection = new ServiceCollection();
    }

    public ContractHostBuilder AddCommand(string[] args) => this.Action(x => x._args = args);

    public ContractHostBuilder AddSingleton<T>() where T : class
    {
        typeof(T)
            .IsAssignableFrom(typeof(IStateService))
            .VerifyAssert(x => x == true, "Must be assignable IStateService");

        _serviceCollection.AddSingleton<T>();
        return this;
    }

    public ContractHostBuilder AddSingleton<T>(Func<IServiceProvider, T> implementationFactory) where T : class
    {
        typeof(T)
            .IsAssignableFrom(typeof(IStateService))
            .VerifyAssert(x => x == true, "Must be assignable IStateService");

        implementationFactory.VerifyNotNull(nameof(implementationFactory));

        _serviceCollection.AddSingleton<T>(service => implementationFactory(service));
        return this;
    }

    public async Task<IContractHost> Build()
    {
        _args.VerifyNotNull($"Args are required, use AddCommand()");

        await _serviceCollection.ConfigureContractService(BuildOption(_args));

        return new ContractHost
        {
            ServiceProvider = _serviceCollection.BuildServiceProvider(),
        };
    }

    private ContractHostOption BuildOption(string[] args)
    {
        args.VerifyNotNull(nameof(args));

        string configFile = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build()
            .Bind<ContractHostOption>()
            .Func(x => x.ConfigFile.VerifyNotEmpty("ConfigFile is required"));

        return new ConfigurationBuilder()
            .AddJsonFile(configFile)
            .AddCommandLine(args)
            .Build()
            .Bind<ContractHostOption>()
            .VerifyBootstrap();
    }
}
