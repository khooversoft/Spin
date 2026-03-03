using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        services.AddSingleton<GraphMapStore>();
        services.AddSingleton<GraphLanguageParser>();
        services.AddSingleton<IGraphClient, GraphQueryExecute>();
        services.TryAddSingleton<LogSequenceNumber>();

        services.AddDataSpace(cnfg =>
        {
            cnfg.Spaces.Add(new SpaceDefinition
            {
                Name = GraphConstants.GraphMap.Key,
                ProviderName = "keyStore",
                BasePath = $"{hostOption.BasePath}/{GraphConstants.GraphMap.BasePath}",
                SpaceFormat = SpaceFormat.Key,
                UseCache = true,
            });

            cnfg.Spaces.Add(new SpaceDefinition
            {
                Name = GraphConstants.File.Key,
                ProviderName = "keyStore",
                BasePath = $"{hostOption.BasePath}/{GraphConstants.File.BasePath}",
                SpaceFormat = SpaceFormat.Hash,
                UseCache = true,
            });

            cnfg.Spaces.Add(new SpaceDefinition
            {
                Name = GraphConstants.Journal.Key,
                ProviderName = "listStore",
                BasePath = $"{hostOption.BasePath}/{GraphConstants.Journal.BasePath}",
                SpaceFormat = SpaceFormat.List,
            });

            cnfg.Add<KeyStoreProvider>("keyStore");
            cnfg.Add<ListStoreProvider>("listStore");
        });

        services.AddKeyStore<GraphSerialization>(GraphConstants.GraphMap.Key);
        services.AddKeyStore(GraphConstants.File.Key);
        services.AddListStore<DataChangeRecord>(GraphConstants.Journal.Key);

        services.AddTransaction(GraphConstants.Transaction.Name, config =>
        {
            config.ListSpaceName = GraphConstants.Journal.Key;
            config.JournalKey = GraphConstants.Journal.Key;
            config.TrxProviders.Add(x => (ITrxProvider)x.GetRequiredKeyedService<IKeyStore>(GraphConstants.File.Key));
        });

        return services;
    }
}


