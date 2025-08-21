using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Models;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class GraphStartup
{
    public static IServiceCollection AddGraphEngine(this IServiceCollection services, Action<GraphHostOption> config)
    {
        services.NotNull();
        config.NotNull();

        var hostOption = new GraphHostOption().Action(x => config(x));
        hostOption.Validate().ThrowOnError("GraphHostOption is invalid");

        services.AddSingleton(hostOption);
        services.AddSingleton<IGraphEngine, GraphEngine>();
        services.AddSingleton<GraphMapCounter>();
        services.AddSingleton<GraphMapDataManager>();
        services.AddSingleton<IGraphClient, GraphQueryExecute>();

        services.AddKeyStore<GraphSerialization>(FileSystemType.Key, config =>
        {
            config.BasePath = $"{hostOption.BasePath}/{GraphConstants.GraphMap.BasePath}";
            config.AddKeyStore();
        });

        services.AddKeyStore<DataETag>(FileSystemType.Hash, config =>
        {
            config.BasePath = $"{hostOption.BasePath}/{GraphConstants.Data.BasePath}";
            config.AddKeyStore();
        });

        services.AddListStore<DataChangeRecord>(config =>
        {
            config.BasePath = $"{hostOption.BasePath}/{GraphConstants.Journal.BasePath}";
        });

        return services;
    }
}


