using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

public class DirectoryConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<DirectoryConnector> _logger;

    public DirectoryConnector(IClusterClient client, ILogger<DirectoryConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Directory}");

        group.MapPost("/addEdge", AddEdge);
        group.MapPost("/addNode", AddNode);
        group.MapPost("/search", Lookup);
        group.MapDelete("/{nodeKey}/edge", RemoveEdgeByNode);
        group.MapDelete("/edge", RemoveEdge);
        group.MapDelete("/{nodeKey}/node", RemoveNode);

        return group;
    }

    private async Task<IResult> AddEdge(DirectoryEdge edge, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!edge.Validate(out var v)) return v.ToResult();

        Option result = await _client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey).AddEdge(edge, traceId);
        return result.ToResult();
    }

    private async Task<IResult> AddNode(DirectoryNode node, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!node.Validate(out var v)) return v.ToResult();

        Option result = await _client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey).AddNode(node, traceId);
        return result.ToResult();
    }

    private async Task<IResult> Lookup(DirectorySearch search, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option<DirectoryResponse> result = await _client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey).Lookup(search, traceId);
        return result.ToResult();
    }

    private async Task<IResult> RemoveEdgeByNode(string nodeKey, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nodeKey = Uri.UnescapeDataString(nodeKey);
        Option result = await _client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey).RemoveEdge(nodeKey, traceId);
        return result.ToResult();
    }

    private async Task<IResult> RemoveEdge(DirectoryEdge edge, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option result = await _client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey).RemoveEdge(edge, traceId);
        return result.ToResult();
    }

    private async Task<IResult> RemoveNode(string nodeKey, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nodeKey = Uri.UnescapeDataString(nodeKey);
        Option result = await _client.GetResourceGrain<IDirectoryActor>(SpinConstants.DirectoryActorKey).RemoveNode(nodeKey, traceId);
        return result.ToResult();
    }
}
