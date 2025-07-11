using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphHostService : IGraphClient, IDisposable
{
    private readonly IHost _host;
    private readonly IGraphEngine _graphEngine;
    private readonly IGraphClient _graphClient;

    public GraphHostService(IHost host, IGraphEngine graphEngine, IGraphClient graphClient, ILogger<GraphHostService> logger)
    {
        _host = host.NotNull();
        _graphEngine = graphEngine.NotNull();
        _graphClient = graphClient.NotNull();
    }

    public IServiceProvider Services => _host.Services;
    public IGraphEngine GraphEngine => _graphEngine;
    public GraphMap Map => GraphEngine.GetMapData().NotNull("no map set").Map;


    public Task<Option<QueryResult>> Execute(string command, ScopeContext context) => _graphClient.Execute(command, context);
    public Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context) => _graphClient.ExecuteBatch(command, context);

    public ScopeContext CreateScopeContext<T>() => new ScopeContext(Services.GetRequiredService<ILogger<T>>());

    public void Dispose() => _host.Dispose();
}

