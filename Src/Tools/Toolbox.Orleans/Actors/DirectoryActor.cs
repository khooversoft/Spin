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

    public DirectoryActor(IGraphHost graphHost, ILogger<DirectoryActor> logger)
    {
        _logger = logger.NotNull();
        _graphHost = graphHost.NotNull();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var context = new ScopeContext(_logger);
        await _graphHost.LoadMap(context);
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<QueryResult>> Execute(string command, ScopeContext context)
    {
        var result = await Execute(command, context);
        return result;
    }

    public async Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context)
    {
        var result = await ExecuteBatch(command, context);
        return result;
    }
}