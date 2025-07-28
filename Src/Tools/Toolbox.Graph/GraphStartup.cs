using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Models;
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

        services.AddDataPipeline<GraphSerialization>(builder =>
        {
            builder.BasePath = $"{hostOption.BasePath}/{GraphConstants.GraphMap.BasePath}";
            builder.AddFileStore();
        });

        services.AddDataPipeline<DataETag>(builder =>
        {
            builder.BasePath = $"{hostOption.BasePath}/{GraphConstants.Data.BasePath}";
            builder.AddFileStore();
        });

        services.AddDataListPipeline<DataChangeRecord>(builder =>
        {
            builder.BasePath = $"{hostOption.BasePath}/{GraphConstants.Journal.BasePath}";
            builder.AddListStore();
        });

        return services;
    }
}


