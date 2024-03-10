using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Data;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;
using Toolbox.Graph;

namespace Toolbox.Identity;

public interface IIdentityActor : IIdentityClient, IGrainWithStringKey
{
}

public class IdentityActor : Grain, IIdentityActor
{
    private readonly ILogger<IdentityActor> _logger;
    private readonly ActorCacheState<GraphMap, GraphSerialization> _state;

    public IdentityActor(
        StateManagement stateManagement,
        [PersistentState("default", ToolboxIdentityConstants.DataLakeProviderName)] IPersistentState<GraphSerialization> state,
        ILogger<IdentityActor> logger
        )
    {
        _logger = logger.NotNull();
        stateManagement.NotNull();

        _state = new ActorCacheState<GraphMap, GraphSerialization>(stateManagement, state, x => x.ToSerialization(), x => x.FromSerialization(), TimeSpan.FromMinutes(15));
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => x == ToolboxIdentityConstants.DirectoryActorKey, x => $"Actor key {x} is not {ToolboxIdentityConstants.DirectoryActorKey}");

        _state.SetName(nameof(IdentityActor), this.GetPrimaryKeyString());
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

        var commandOption = map.Execute(command, context);
        if (commandOption.StatusCode.IsError()) return commandOption;

        GraphQueryResults commandResult = commandOption.Return();

        bool isMapModified = commandResult.Items.Any(x => x.CommandType != CommandType.Select);
        if (!isMapModified) return commandResult;

        context.Location().LogInformation("Directory command modified graph, writing changes");
        await _state.SetState(map, context);

        return commandResult;
    }
}
