using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;


public interface IDirectoryActor : IGrainWithStringKey
{
    Task<Option> AddEdge(GraphEdge edge, string traceId);
    Task<Option> AddNode(GraphNode node, string traceId);
    Task Clear(string traceId);
    Task<Option<GraphQueryResult>> Query(DirectoryQuery search, string traceId);
    Task<Option<GraphQueryResult>> Remove(DirectoryQuery search, string traceId);
    Task<Option> Update(DirectoryEdgeUpdate node, string traceId);
    Task<Option> Update(DirectoryNodeUpdate node, string traceId);
}

public class DirectoryActor : Grain, IDirectoryActor
{
    private readonly ILogger<DirectoryActor> _logger;
    private readonly IPersistentState<GraphSerialization> _state;
    private GraphMap _map = new GraphMap();

    public DirectoryActor(
        [PersistentState(stateName: SpinConstants.Ext.BlockStorage, storageName: SpinConstants.SpinStateStore)] IPersistentState<GraphSerialization> state,
        ILogger<DirectoryActor> logger
        )
    {
        _state = state.NotNull();
        _logger = logger.NotNull();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString()
            .Assert(x => x == SpinConstants.DirectoryActorKey, x => $"Actor key {x} is invalid, must = {SpinConstants.DirectoryActorKey}");

        switch (_state.RecordExists)
        {
            case true: await ReadGraphFromStorage(); break;
            case false: await SetGraphToStorage(); break;
        }

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> AddEdge(GraphEdge edge, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Adding edge, edge={edge}", edge);

        var option = _map.Edges.Add(edge);
        if (option.IsError()) return option;

        await SetGraphToStorage();
        return StatusCode.OK;
    }

    public async Task<Option> AddNode(GraphNode node, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Adding node, node={node}", node);

        var option = _map.Nodes.Add(node);
        if (option.IsError()) return option;

        await SetGraphToStorage();
        return StatusCode.OK;
    }

    //public async Task<Option> Batch(DirectoryBatch batch, string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    context.Location().LogInformation("Processing batch, batch={batch}", batch);

    //    // Working map
    //    GraphMap workingMap = GraphMap.FromJson(_map.ToJson());
    //    Option option;

    //    foreach (var item in batch.Items)
    //    {
    //        switch (item)
    //        {
    //            case DirectoryNode node:
    //                option = workingMap.Nodes.Add(node.ConvertTo());
    //                if (option.IsError()) return option;
    //                break;

    //            case DirectoryEdge edge:
    //                option = workingMap.Edges.Add(edge.ConvertTo());
    //                if (option.IsError()) return option;
    //                break;

    //            case RemoveNode removeNode:
    //                workingMap.Nodes.Remove(removeNode.NodeKey);
    //                break;

    //            case RemoveEdge removeEdge:
    //                workingMap.Edges.Remove(removeEdge.EdgeKey);
    //                break;
    //        }
    //    }
    //}

    public async Task Clear(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Clearing graph");

        _map.Clear();
        await SetGraphToStorage();
    }

    public Task<Option<GraphQueryResult>> Query(DirectoryQuery search, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!search.Validate(out var v)) return v.ToOptionStatus<GraphQueryResult>().ToTaskResult();
        context.Location().LogInformation("Lookup edge, search={search}", search);

        GraphQueryResult result = _map.Query().Execute(search.GraphQuery);
        if (result.StatusCode.IsError()) return new Option<GraphQueryResult>(result.StatusCode, result.Error).ToTaskResult();

        return result.ToOption().ToTaskResult();
    }

    public async Task<Option<GraphQueryResult>> Remove(DirectoryQuery search, string traceId)
    {
        if (!search.Validate(out var v)) return v.ToOptionStatus<GraphQueryResult>();
        var context = new ScopeContext(traceId, _logger);

        Option<GraphQueryResult> result = await Query(search, traceId);
        if (result.IsError()) return result;

        GraphQueryResult response = result.Return();
        if (response.Items.Count == 0) return StatusCode.NotFound;

        context.Location().LogInformation("Removing nodes/edges, GraphQueryResult={graphQueryResult}", response);

        response.Items.OfType<GraphNode>().ForEach(x => _map.Nodes.Remove(x.Key));
        response.Items.OfType<GraphEdge>().ForEach(x => _map.Edges.Remove(x.Key));

        await SetGraphToStorage();
        return response;
    }

    public Task<Option> Update(DirectoryEdgeUpdate model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Updating edge, model={model}", model);

        _map.Edges.Update(model.ConvertTo(), x => x with
        {
            EdgeType = model.UpdateEdgeType ?? x.EdgeType,
            Tags = x.Tags.Set(model.UpdateTags)
        });

        return new Option(StatusCode.OK).ToTaskResult();
    }

    public Task<Option> Update(DirectoryNodeUpdate model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Updating node, model={model}", model);

        _map.Nodes.Update(model.ConvertTo(), x => x with
        {
            Tags = x.Tags.Set(model.UpdateTags)
        });

        return new Option(StatusCode.OK).ToTaskResult();
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