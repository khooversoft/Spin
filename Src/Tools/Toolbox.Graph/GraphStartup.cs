using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Graph;

public static class GraphStartup
{
    public static IServiceCollection AddGraphMemoryTrace(this IServiceCollection services)
    {
        services.NotNull();
        services.AddSingleton<IChangeTrace, InMemoryChangeTrace>();
        services.AddSingleton<IGraphFileStore, FileStoreTraceShim>();

        return services;
    }

    public static IServiceCollection AddGraphFileStore(this IServiceCollection services)
    {
        services.NotNull().AddSingleton(services => (IGraphFileStore)services.GetRequiredService<IFileStore>());

        return services;
    }
}
