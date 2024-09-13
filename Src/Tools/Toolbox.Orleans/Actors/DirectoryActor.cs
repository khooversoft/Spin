using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
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
    private readonly ActorCacheState<GraphMap, GraphSerialization> _state;
    private readonly IGraphFileStore _graphFileStore;

    public DirectoryActor(
        [PersistentState("json", OrleansConstants.StorageProviderName)] IPersistentState<GraphSerialization> state,
        IGraphFileStore graphFileStore,
        ILogger<DirectoryActor> logger
        )
    {
        _logger = logger.NotNull();
        _graphFileStore = graphFileStore.NotNull();

        _state = new ActorCacheState<GraphMap, GraphSerialization>(state, x => x.ToSerialization(), x => x.FromSerialization(), TimeSpan.FromMinutes(15));
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (!_state.RecordExists) await _state.SetState(new GraphMap());
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<QueryResult>> Execute(string command, ScopeContext context)
    {
        var result = await ExecuteBatch(command, context);
        if (result.IsError()) return result.ToOptionStatus<QueryResult>();

        return result.Return().Items.First();
    }

    public async Task<Option<QueryBatchResult>> ExecuteBatch(string command, ScopeContext context)
    {
        //context = context.With(_logger);
        //if (command.IsEmpty()) return (StatusCode.BadRequest, "Command is empty");
        //context.Location().LogInformation("Command, search={search}", command);

        //GraphMap map = (await _state.GetState()).ThrowOnError("Failed to get state").Return();
        //var graphContext = new GraphTrxContext(map, _graphFileStore, context);
        //var commandOption = await GraphCommand.Execute(graphContext, command);
        //if (commandOption.IsError()) return commandOption;

        //GraphQueryResults commandResult = commandOption.Return();
        //if (!commandResult.IsMutating) return commandResult;

        //context.Location().LogInformation("Directory command modified graph, writing changes");
        //await _state.SetState(map);

        //return commandResult;
        return StatusCode.BadRequest;
    }
}