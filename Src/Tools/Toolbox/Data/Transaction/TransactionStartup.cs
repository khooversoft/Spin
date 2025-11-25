using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class TransactionStartup
{
    public static TransactionStartupContext AddTransactionServices(this IServiceCollection services, TransactionManagerOption option)
    {
        option.NotNull();
        services.AddKeyedSingleton<TransactionProviderRegistry>(option.Name);
        services.AddKeyedSingleton<TransactionManager>(option.Name, (services, obj) => ActivatorUtilities.CreateInstance<TransactionManager>(services, option));
        services.TryAddSingleton<LogSequenceNumber>();

        return new TransactionStartupContext(services, option);
    }
}


public sealed record TransactionStartupContext
{
    public TransactionStartupContext(IServiceCollection serviceCollection, TransactionManagerOption option)
    {
        ServiceCollection = serviceCollection.NotNull();
        Option = option;
    }

    public TransactionManagerOption Option { get; }
    public IServiceCollection ServiceCollection { get; }
}

public static class TransactionStartupContextExtensions
{
    public static TransactionProviderRegistry Register(this TransactionManagerOption option, IServiceProvider serviceProvider, ITransactionRegister transactionRegister)
    {
        option.NotNull();
        serviceProvider.NotNull();
        transactionRegister.NotNull();

        var registry = serviceProvider.GetKeyedService<TransactionProviderRegistry>(option.Name).NotNull();
        registry.Add(transactionRegister);
        return registry;
    }
}