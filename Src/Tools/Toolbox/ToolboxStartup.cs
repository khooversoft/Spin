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

        var c = new DataSpaceConfig();
        config(c);

        services.AddSingleton<DataSpace>(services =>
        {
            var option = c.Build(services);
            return ActivatorUtilities.CreateInstance<DataSpace>(services, option);
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

    //public static IServiceCollection AddSequenceStore<T>(this IServiceCollection services, string spaceName)
    //{
    //    services.NotNull();
    //    spaceName.NotEmpty();

    //    services.AddSingleton<ISequenceStore<T>>(services =>
    //    {
    //        var dataSpace = services.GetRequiredService<DataSpace>();
    //        return dataSpace.GetSequenceStore<T>(spaceName);
    //    });

    //    return services;
    //}

    //public static IServiceCollection AddSequenceLimit<T>(this IServiceCollection services, Action<SequenceSizeLimitOption<T>> config)
    //{
    //    services.NotNull();
    //    config.NotNull();

    //    var option = new SequenceSizeLimitOption<T>();
    //    config?.Invoke(option);

    //    services.AddSingleton<SequenceSizeLimitOption<T>>(option);
    //    services.AddSingleton<SequenceSizeLimit<T>>(service =>
    //    {
    //        var sequenceStore = service.GetRequiredService<ISequenceStore<T>>();
    //        var limiter = ActivatorUtilities.CreateInstance<SequenceSizeLimit<T>>(service, sequenceStore);

    //        var space = service.GetRequiredService<ISequenceStore<T>>() as SequenceSpace<T> ?? throw new ArgumentException("ISequenceStore<T> is not SequenceSpace<T>");
    //        space.SetLimiter(limiter);
    //        return limiter;
    //    });

    //    return services;
    //}

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
