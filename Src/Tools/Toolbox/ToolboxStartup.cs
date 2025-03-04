using Microsoft.Extensions.DependencyInjection;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox;

public static class ToolboxStartup
{
    public static IServiceCollection AddInMemoryFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton<IFileStore, InMemoryFileStore>();
        return services;
    }

    public static IServiceCollection AddLocalFileStore(this IServiceCollection services, LocalFileStoreOption option)
    {
        services.NotNull();
        option.NotNull().Validate().ThrowOnError("Invalid LocalFileStoreOption");

        services.AddSingleton(option);
        services.AddSingleton<IFileStore, LocalFileStore>();
        return services;
    }

    public static IServiceCollection AddJournalLog(this IServiceCollection services, string key, JournalFileOption option)
    {
        services.NotNull();
        key.NotEmpty();
        option.NotNull().Validate().ThrowOnError();

        services.AddKeyedSingleton<IJournalFile>(key, (iServices, _) =>
        {
            return ActivatorUtilities.CreateInstance<JournalFile>(iServices, option);
        });

        return services;
    }
}
