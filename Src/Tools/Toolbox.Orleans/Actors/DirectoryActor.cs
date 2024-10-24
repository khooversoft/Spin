using Microsoft.Extensions.Logging;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public interface IDirectoryActor : IGraphClient, IGrainWithStringKey
{
}

public class DirectoryActor : Grain, IDirectoryActor
{
    private readonly ILogger<DirectoryActor> _logger;
    private readonly IGraphHost _graphHost;
    private readonly IPersistentState<DataETag> _state;

    public DirectoryActor(
        IGraphHost graphHost,
        [PersistentState("json", OrleansConstants.StorageProviderName)] IPersistentState<DataETag> state,
        ILogger<DirectoryActor> logger)
    {
        _graphHost = graphHost.NotNull();
        _state = state.NotNull();
        _logger = logger.NotNull();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var context = new ScopeContext(_logger);
        await _graphHost.LoadMap(context);
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<QueryResult>> Execute(string command, ScopeContext context)
    {
        var result = await QueryExecution.Execute(_graphHost, command, context);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items.Last();
    }

    public async Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context)
    {
        var result = await QueryExecution.Execute(_graphHost, command, context);
        return result;
    }
}