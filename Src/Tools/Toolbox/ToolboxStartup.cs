using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
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
}
