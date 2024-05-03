using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class GraphEngineBuilder
{
    public GraphEngineBuilder(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider.NotNull();

    public IServiceProvider ServiceProvider { get; private set; }
    public IChangeTrace? ChangeTrace { get; private set; }
    public IFileStore? FileStore { get; private set; }
    public GraphMap? Map { get; private set; }

    public GraphEngineBuilder AddServiceProvider(IServiceProvider serviceProvider) => this.Action(_ => ServiceProvider = serviceProvider);
    public GraphEngineBuilder AddFileStore(IFileStore fileStore) => this.Action(_ => FileStore = fileStore);
    public GraphEngineBuilder AddChangeTrace(IChangeTrace changeTrace) => this.Action(_ => ChangeTrace = changeTrace);
    public GraphEngineBuilder AddMap(GraphMap map) => this.Action(_ => Map = map);

    public GraphEngineBuilder AddMemoryFile() => this.Action(_ => FileStore = ActivatorUtilities.CreateInstance<InMemoryFileStore>(ServiceProvider));

    public GraphEngine Build()
    {
        Map.NotNull("is required");
        FileStore.NotNull("is required");

        var context = new GraphContext
        {
            ChangeTrace = ChangeTrace,
            FileStore = FileStore,
            Map = Map,
        };
    }
}

public interface IGraphClient
{
    IGraphCommand? Command { get; }
    IGraphEntity? Entity { get; }
    IGraphStore? GraphStore { get; }
}


public class GraphEngine
{
    public GraphEngine(IGraphContext graphContext)
    {
        GraphContext = graphContext;
    }

    public IGraphContext GraphContext { get; }
}