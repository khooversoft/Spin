using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Extensions;
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
        services.AddSingleton<GraphMapDataManager>();
        services.AddSingleton<GraphLanguageParser>();
        services.AddSingleton<IGraphClient, GraphQueryExecute>();

        services.AddDataSpace(cnfg =>
        {
            cnfg.Spaces.Add(new SpaceDefinition
            {
                Name = "graphDb",
                ProviderName = "fileStore",
                BasePath = $"{hostOption.BasePath}/{GraphConstants.GraphMap.BasePath}",
                SpaceFormat = SpaceFormat.Key,
                UseCache = true,
            });

            cnfg.Spaces.Add(new SpaceDefinition
            {
                Name = "graphFile",
                ProviderName = "fileStore",
                BasePath = $"{hostOption.BasePath}/{GraphConstants.GraphMap.BasePath}",
                SpaceFormat = SpaceFormat.Hash,
                UseCache = true,
            });

            cnfg.Spaces.Add(new SpaceDefinition
            {
                Name = "journal",
                ProviderName = "listStore",
                BasePath = $"{hostOption.BasePath}/{GraphConstants.Journal.BasePath}",
                SpaceFormat = SpaceFormat.List,
            });

            cnfg.Add<KeyStoreProvider>("fileStore");
            cnfg.Add<ListStoreProvider>("listStore");
        });

        services.AddKeyStore("graphFile");
        services.AddKeyStore<DataETag>("graphFile");

        services.AddKeyStore<GraphSerialization>("graphDb", option =>
        {
            option.Serializer = x => JsonSerializer.Serialize(x, GraphJsonContext.Default.GraphSerialization);
            option.Deserializer = x => JsonSerializer.Deserialize(x, GraphJsonContext.Default.GraphSerialization);
        });

        services.AddListStore<DataChangeRecord>("journal", option =>
        {
            option.Serializer = x => JsonSerializer.Serialize(x, DataChangeRecordJsonContext.Default.DataChangeRecord);
            option.Deserializer = x => JsonSerializer.Deserialize(x, DataChangeRecordJsonContext.Default.DataChangeRecord);
        });

        return services;
    }
}


