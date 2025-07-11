using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface IDirectoryActor : IGrainWithStringKey
{
    Task<Option> Clear(string principalId, string traceId);
    Task<Option<GraphQueryResults>> Execute(string command, string traceId);
}

public class DirectoryActor : Grain, IDirectoryActor
{
    private readonly ILogger<DirectoryActor> _logger;
    private readonly ActorCacheState<GraphMap, GraphSerialization> _state;

    public DirectoryActor(
        StateManagement stateManagement,
        [PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<GraphSerialization> state,
        ILogger<DirectoryActor> logger
        )
    {
        _logger = logger.NotNull();
        stateManagement.NotNull();

        _state = new ActorCacheState<GraphMap, GraphSerialization>(stateManagement, state, x => x.ToSerialization(), x => x.FromSerialization(), TimeSpan.FromMinutes(15));
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => x == NBlogConstants.DirectoryActorKey, x => $"Actor key {x} is not {NBlogConstants.DirectoryActorKey}");

        _state.SetName(nameof(DirectoryActor), this.GetPrimaryKeyString());
        if (!_state.RecordExists) await _state.SetState(new GraphMap(), new ScopeContext(_logger));

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Clear(string principalId, string traceId)
    {
        principalId.NotEmpty();

        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Clearing graph");

        return await _state.Clear();
    }

    public async Task<Option<GraphQueryResults>> Execute(string command, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (command.IsEmpty()) return (StatusCode.BadRequest, "Command is empty");
        context.Location().LogInformation("Command, search={search}", command);

        GraphMap map = (await _state.GetState(context)).ThrowOnError("Failed to get state").Return();

        var commandOption = map.Execute(command);
        if (commandOption.StatusCode.IsError()) return commandOption;

        GraphQueryResults commandResult = commandOption.Return();

        bool isMapModified = commandResult.Items.Any(x => x.CommandType != CommandType.Select);
        if (!isMapModified) return commandResult;

        context.Location().LogInformation("Directory command modified graph, writing changes");
        await _state.SetState(map, context);

        return commandResult;
    }
}