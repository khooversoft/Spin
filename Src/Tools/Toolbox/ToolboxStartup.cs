using Microsoft.Extensions.DependencyInjection;
using Toolbox.Store;
using Toolbox.Tools;

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

        return services;
    }

    public static IServiceCollection AddDataSpace(this IServiceCollection services, Action<DataSpaceOption> config)
    {
        services.NotNull();
        config.NotNull();

        var c = new DataSpaceOption();
        config(c);

        services.AddSingleton<LockManager>();
        services.AddSingleton<DataSpace>(services =>);
        return services;
    }

    //public static IServiceCollection AddInMemoryFileStore(this TransactionStartupContext trxStartupContext, MemoryStore? memoryStore = null)
    //{
    //    trxStartupContext.ServiceCollection.AddSingleton<IFileStore, InMemoryFileStore>();

    //    switch (memoryStore)
    //    {
    //        case null:
    //            trxStartupContext.ServiceCollection.AddSingleton<MemoryStore>(services => ActivatorUtilities.CreateInstance<MemoryStore>(services, trxStartupContext.Option));
    //            break;

    //        default:
    //            trxStartupContext.ServiceCollection.AddKeyedSingleton<MemoryStore>(
    //                trxStartupContext.Option.Name,
    //                (services, obj) => ActivatorUtilities.CreateInstance<MemoryStore>(services, trxStartupContext.Option)
    //                );
    //            break;
    //    }

    //    return trxStartupContext.ServiceCollection;
    //}
}
