using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Toolbox.Data;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class ToolboxStartup
{
    public static IServiceCollection AddInMemoryFileStore(this IServiceCollection services, MemoryStore? memoryStore = null)
    {
        services.NotNull();

        services.AddSingleton<IFileStore, InMemoryFileStore>();

        switch (memoryStore)
        {
            case null: services.AddSingleton<MemoryStore>(); break;
            default: services.AddSingleton(memoryStore); break;
        }
        ;

        return services;
    }

    public static IServiceCollection AddLocalFileStore(this IServiceCollection services, LocalFileStoreOption option)
    {
        services.NotNull();
        option.NotNull().Validate().ThrowOnError("Invalid LocalFileStoreOption");

        services.AddSingleton(option);
        //services.AddSingleton<IFileStore, LocalFileStore>();
        return services;
    }

    public static IServiceCollection AddJournalLog(this IServiceCollection services, string key, JournalFileOption option)
    {
        services.NotNull();
        key.NotEmpty();
        option.NotNull().Validate().ThrowOnError();

        services.AddSingleton(option);
        services.AddKeyedSingleton<IJournalFile, JournalFile>(key);

        return services;
    }

    public static IServiceCollection AddDataClient(this IServiceCollection services, Action<DataClientBuilder> config, string? name = "default")
    {
        services.NotNull();
        config.NotNull();

        var builder = new DataClientBuilder(services) { Name = name };
        config(builder);
        builder.Name.NotEmpty("Name is required");

        services.TryAddSingleton<DataClientFactory>();
        services.AddKeyedSingleton(builder.Name, builder);
        return services;
    }

    public static IServiceCollection AddDataClient<T>(this IServiceCollection services, Action<DataClientBuilder>? config = null)
    {
        services.NotNull();
        config.NotNull();

        var builder = new DataClientBuilder(services) { Name = typeof(T).Name };
        config(builder);

        services.TryAddSingleton<DataClientFactory>();
        services.AddKeyedSingleton(builder.Name, builder);

        builder.Services.AddTransient<IDataClient<T>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DataClientFactory>();
            return factory.Create<T>();
        });

        return services;
    }
}
