using Microsoft.Extensions.DependencyInjection;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox;

public static class ToolboxStartup
{
    public static IServiceCollection AddInMemoryFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton<IFileStore, InMemoryFileStore>();
        return services;
    }
}
