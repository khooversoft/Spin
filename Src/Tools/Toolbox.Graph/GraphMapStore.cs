using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphMapStore
{
    Task<Option> Get(IGraphContext graphContext, ScopeContext context);
    Task<Option> Set(IGraphContext graphContext, ScopeContext context);
}

public class GraphMapStore : IGraphMapStore
{
    private readonly SemaphoreSlim _resetEvent = new SemaphoreSlim(1, 1);
    private readonly ILogger<GraphMapStore> _logger;
    private readonly string _mapDatabasePath;

    public GraphMapStore(string mapDatabasePath, ILogger<GraphMapStore> logger)
    {
        _mapDatabasePath = mapDatabasePath.NotEmpty();
        _logger = logger.NotNull();
    }

    public async Task<Option> Get(IGraphContext graphContext, ScopeContext context)
    {
        graphContext.NotNull();
        context = context.With(_logger);

        await _resetEvent.WaitAsync(context.CancellationToken);
        try
        {
            context.LogInformation("Reading graph data file={mapDatabasePath}", _mapDatabasePath);

            var readMapSerializer = await graphContext.FileStore.Get(_mapDatabasePath, context);
            if (readMapSerializer.IsError()) return readMapSerializer.ToOptionStatus();

            GraphSerialization graphSerialization = readMapSerializer.Return().ToObject<GraphSerialization>();
            GraphMap newMap = graphSerialization.FromSerialization();

            context.LogInformation("Read graph data file={mapDatabasePath}", _mapDatabasePath);
            graphContext.SetMap(newMap);

            return StatusCode.OK;
        }
        finally
        {
            _resetEvent.Release();
        }
    }

    public async Task<Option> Set(IGraphContext graphContext, ScopeContext context)
    {
        context = context.With(_logger);

        await _resetEvent.WaitAsync(context.CancellationToken);
        try
        {
            context.LogInformation("Writing graph data file={mapDatabasePath}", _mapDatabasePath);

            GraphSerialization graphSerialization = graphContext.Map.ToSerialization();
            var writeMapSerializer = await graphContext.FileStore.Set(_mapDatabasePath, graphSerialization.ToDataETag(), context);
            if (writeMapSerializer.IsError()) return writeMapSerializer.ToOptionStatus();

            context.LogInformation("Write graph data file={mapDatabasePath}", _mapDatabasePath);
            return StatusCode.OK;
        }
        finally
        {
            _resetEvent.Release();
        }
    }
}
