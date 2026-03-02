using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Data;
using Toolbox.Store;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class ToolboxStartup
{
    private sealed record DataSpaceConfigRegistration(Action<DataSpaceConfig> Configure);

    public static IServiceCollection AddTelemetry(this IServiceCollection services, Action<TelemetryOption>? options = null)
    {
        services.NotNull();

        var option = new TelemetryOption();
        options?.Invoke(option);

        services.AddSingleton<TelemetryOption>(option);
        services.AddSingleton<TelemetryCollector>();
        services.AddSingleton<ITelemetry, TelemetryFactory>();

        return services;
    }

    public static IServiceCollection AddInMemoryKeyStore(this IServiceCollection services, MemoryStore? memoryStore = null)
    {
        services.NotNull();

        services.AddSingleton<IKeyStore, MemoryKeyStore>();

        switch (memoryStore)
        {
            case null: services.AddSingleton<MemoryStore>(); break;
            default: services.AddSingleton(memoryStore); break;
        }

        return services;
    }

    public static IServiceCollection AddDataSpace(this IServiceCollection services, Action<DataSpaceConfig> config)
    {
        services.NotNull();
        config.NotNull();

        services.AddSingleton(new DataSpaceConfigRegistration(config));

        services.TryAddSingleton<DataSpace>(service =>
        {
            var dataSpace = ActivatorUtilities.CreateInstance<DataSpace>(service);

            foreach (var registration in service.GetServices<DataSpaceConfigRegistration>())
            {
                var c = new DataSpaceConfig();
                registration.Configure(c);

                var option = c.Build(service);
                dataSpace.AddSpace(option);
            }

            return dataSpace;
        });

        return services;
    }

    public static IServiceCollection AddKeyStore(this IServiceCollection services, string spaceName)
    {
        services.NotNull();
        spaceName.NotEmpty();

        services.AddKeyedSingleton<IKeyStore>(spaceName, (services, k) =>
        {
            var dataSpace = services.GetRequiredService<DataSpace>();
            return dataSpace.GetFileStore(spaceName);
        });

        return services;
    }

    public static IServiceCollection AddKeyStore<T>(this IServiceCollection services, string spaceName)
    {
        services.NotNull();
        spaceName.NotEmpty();

        services.AddSingleton<IKeyStore<T>>(services =>
        {
            var dataSpace = services.GetRequiredService<DataSpace>();
            return dataSpace.GetFileStore<T>(spaceName);
        });

        return services;
    }

    public static IServiceCollection AddListStore<T>(this IServiceCollection services, string spaceName)
    {
        services.NotNull();
        spaceName.NotEmpty();

        services.AddSingleton<IListStore<T>>(services =>
        {
            var dataSpace = services.GetRequiredService<DataSpace>();
            return dataSpace.GetListStore<T>(spaceName);
        });

        return services;
    }
    public static IServiceCollection AddTransaction(this IServiceCollection services, string name, Action<TransactionOption> config)
    {
        name.NotEmpty();

        var option = new TransactionOption();
        config(option);
        option.NotNull().Validate().ThrowOnError();

        services.TryAddSingleton<LogSequenceNumber>();
        services.AddKeyedSingleton<TransactionOption>(name, option);

        services.AddKeyedSingleton<Transaction>(name, (service, _) =>
        {
            var option = service.GetRequiredKeyedService<TransactionOption>(name);

            var trxProviders = option.TrxProviders
                .Select(f => f(service))
                .ToList();

            var trx = ActivatorUtilities.CreateInstance<Transaction>(service, option);
            foreach (var p in trxProviders) trx.Providers.Enlist(p);

            return trx;
        });

        return services;
    }
}
