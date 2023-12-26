using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface IDirectoryActor : IGrainWithStringKey
{
    Task<Option> Clear(string principalId, string traceId);
    Task<Option<GraphCommandResults>> Execute(string command, string traceId);
}

public class DirectoryActor : Grain, IDirectoryActor
{
    private readonly ILogger<DirectoryActor> _logger;
    private readonly IPersistentState<GraphSerialization> _state;
    private GraphMap _map = new GraphMap();

    public DirectoryActor(
        [PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<GraphSerialization> state,
        ILogger<DirectoryActor> logger
        )
    {
        _state = state.NotNull();
        _logger = logger.NotNull();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => x == NBlogConstants.DirectoryActorKey, x => $"Actor key {x} is not {NBlogConstants.DirectoryActorKey}");

        switch (_state.RecordExists)
        {
            case true: await ReadGraphFromStorage(); break;
            case false: await SetGraphToStorage(); break;
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Clear(string principalId, string traceId)
    {
        principalId.NotEmpty();

        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Clearing graph");

        _map.Clear();
        await SetGraphToStorage();

        return StatusCode.OK;
    }

    public async Task<Option<GraphCommandResults>> Execute(string command, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (command.IsEmpty()) return (StatusCode.BadRequest, "Command is empty");
        context.Location().LogInformation("Command, search={search}", command);

        var commandOption = _map.Command().Execute(command);
        if (commandOption.StatusCode.IsError()) return commandOption.ToOptionStatus<GraphCommandResults>();

        GraphCommandExceuteResults commandResult = commandOption.Return();

        bool isMapModified = commandResult.Items.Any(x => x.CommandType != CommandType.Select);
        if (!isMapModified) return commandResult.ConvertTo();

        context.Location().LogInformation("Directory command modified graph, writing changes");

        _map = commandResult.GraphMap;
        await SetGraphToStorage();

        return commandResult.ConvertTo();
    }

    private async Task ReadGraphFromStorage(bool forceRead = false)
    {
        _state.RecordExists.Assert(x => x == true, "Record does not exist");

        if (forceRead) await _state.ReadStateAsync();
        _map = _state.State.FromSerialization();
    }

    private async Task SetGraphToStorage()
    {
        _state.State = _map.NotNull().ToSerialization();
        await _state.WriteStateAsync();
    }
}