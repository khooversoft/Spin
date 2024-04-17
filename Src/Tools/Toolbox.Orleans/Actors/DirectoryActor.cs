using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public interface IDirectoryActor : IGrainWithStringKey
{
    Task<Option<GraphQueryResults>> Execute(string command, string traceId);
    Task<Option<GraphQueryResult>> ExecuteScalar(string command, string traceId);
}

public class DirectoryActor : Grain, IDirectoryActor
{
    private readonly ILogger<DirectoryActor> _logger;
    private readonly ActorCacheState<GraphMap, GraphSerialization> _state;

    public DirectoryActor(
        [PersistentState("json", OrleansConstants.StorageProviderName)] IPersistentState<GraphSerialization> state,
        ILogger<DirectoryActor> logger
        )
    {
        _logger = logger.NotNull();

        _state = new ActorCacheState<GraphMap, GraphSerialization>(state, x => x.ToSerialization(), x => x.FromSerialization(), TimeSpan.FromMinutes(15));
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (!_state.RecordExists) await _state.SetState(new GraphMap(), new ScopeContext(_logger));
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<GraphQueryResults>> Execute(string command, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (command.IsEmpty()) return (StatusCode.BadRequest, "Command is empty");
        context.Location().LogInformation("Command, search={search}", command);

        GraphMap map = (await _state.GetState(context)).ThrowOnError("Failed to get state").Return();

        var commandOption = map.Execute(command, context);
        if (commandOption.StatusCode.IsError()) return commandOption;

        GraphQueryResults commandResult = commandOption.Return();
        if (!commandResult.IsMutating) return commandResult;

        context.Location().LogInformation("Directory command modified graph, writing changes");
        await _state.SetState(map, context);

        return commandResult;
    }

    public async Task<Option<GraphQueryResult>> ExecuteScalar(string command, string traceId)
    {
        var result = await Execute(command, traceId);
        if( result.IsError()) return result.ToOptionStatus<GraphQueryResult>();

        return result.Return().Items.First();
    }
}