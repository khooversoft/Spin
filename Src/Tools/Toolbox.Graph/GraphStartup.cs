using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
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

        services.AddSingleton<GraphHostOption>(hostOption);
        services.AddSingleton<IGraphEngine, GraphEngine>();
        services.AddSingleton<GraphMapCounter>();
        services.AddSingleton<GraphMapDataManager>();
        services.AddSingleton<IGraphClient, GraphQueryExecute>();

        services.AddDataPipeline<GraphSerialization>(GraphConstants.GraphMap.PipelineName, builder =>
        {
            builder.BasePath = $"{hostOption.BasePath}/{GraphConstants.GraphMap.BasePath}";
            builder.AddFileStore();
        });

        services.AddDataPipeline<DataETag>(GraphConstants.Data.PipelineName, builder =>
        {
            builder.BasePath = $"{hostOption.BasePath}/{GraphConstants.Data.BasePath}";
            builder.AddFileStore();
        });

        services.AddDataPipeline<DataChangeRecord>(GraphConstants.Journal.PipelineName, builder =>
        {
            builder.BasePath = $"{hostOption.BasePath}/{GraphConstants.Journal.BasePath}";
            builder.AddListStore();
        });

        return services;
    }
}


