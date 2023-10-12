using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    Task<Option> AddEdge(DirectoryEdge edge, string traceId);
    Task<Option> AddNode(DirectoryNode node, string traceId);
    Task<Option<DirectoryResponse>> Lookup(DirectorySearch search, string traceId);
    Task<Option> RemoveEdge(string nodeKey, string traceId);
    Task<Option> RemoveEdge(DirectoryEdge edge, string traceId);
    Task<Option> RemoveNode(string nodeKey, string traceId);
}

public class DirectoryActor : Grain, IDirectoryActor
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<DirectoryActor> _logger;
    private readonly IPersistentState<GraphSerialization> _state;
    private GraphMap _map = new GraphMap();

    public DirectoryActor(
        [PersistentState(stateName: SpinConstants.Extension.BlockStorage, storageName: SpinConstants.SpinStateStore)] IPersistentState<GraphSerialization> state,
        IClusterClient clusterClient,
        ILogger<DirectoryActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString()
            .Assert(x => x == SpinConstants.DirectoryActorKey, x => $"Actor key {x} is invalid, must = {SpinConstants.DirectoryActorKey}");

        if (_state.RecordExists) await ReadGraphFromStorage();
        await base.OnActivateAsync(cancellationToken);
    }


    public async Task<Option> AddEdge(DirectoryEdge edge, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Adding edge, edge={edge}", edge);

        var option = _map.Edges.Add(new GraphEdge<string>(edge.FromKey, edge.ToKey, edge.Tags));
        if (option.IsError()) return option;

        await SetGraphToStorage();
        return StatusCode.OK;
    }

    public async Task<Option> AddNode(DirectoryNode node, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Adding node, node={node}", node);

        var option = _map.Nodes.Add(new GraphNode<string>(node.Key, node.Tags));
        if (option.IsError()) return option;

        await SetGraphToStorage();
        return StatusCode.OK;
    }

    public Task<Option<DirectoryResponse>> Lookup(DirectorySearch search, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Lookup edge, search={search}", search);

        IReadOnlyList<GraphNode<string>> nodeQuery = search.NodeKey switch
        {
            null => Array.Empty<GraphNode<string>>(),
            var v => _map.Query()
                .Nodes(x => x.Key == v && x.Tags.Has(search.NodeTags))
                .HasEdge(x => x.Tags.Has(search.EdgeTags)).Nodes,
        };

        IReadOnlyList<GraphEdge<string>> edgeQuery = (search.FromKey, search.ToKey) switch
        {
            (string FromKey, string ToKey) v => _map.Query()
                .Edges(x => x.FromNodeKey == v.FromKey && x.ToNodeKey == v.ToKey && x.Tags.Has(search.EdgeTags))
                .HasNode(x => x.Tags.Has(search.NodeKey))
                .Edges,

            _ => Array.Empty<GraphEdge<string>>(),
        };

        var result = new DirectoryResponse
        {
            Nodes = nodeQuery.Select(x => new DirectoryNode(x.Key, x.Tags.ToString())).ToArray(),
            Edges = edgeQuery.Select(x => new DirectoryEdge(x.FromNodeKey, x.ToNodeKey, x.Tags.ToString())).ToArray(),
        };

        return result.ToOption().ToTaskResult();
    }

    public async Task<Option> RemoveEdge(string nodeKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Removing edge, nodeKey={nodeKey}", nodeKey);

        bool state = _map.Edges.Remove(nodeKey);
        if (state) await SetGraphToStorage();

        return (state ? StatusCode.OK : StatusCode.NotFound);
    }

    public async Task<Option> RemoveEdge(DirectoryEdge edge, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        if (!edge.Validate(out var v)) return v;
        context.Location().LogInformation("Removing edge, edge={edge}", edge);

        var states = _map.Edges.Remove(edge.FromKey, edge.ToKey);
        if (states.Count > 0) await SetGraphToStorage();

        return (states.Count > 0 ? StatusCode.OK : StatusCode.NotFound);
    }

    public async Task<Option> RemoveNode(string nodeKey, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Removing node, nodeKey={nodeKey}", nodeKey);

        bool state = _map.Nodes.Remove(nodeKey);
        if (state) await SetGraphToStorage();

        return (state ? StatusCode.OK : StatusCode.NotFound);
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