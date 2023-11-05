using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Data;
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
        group.MapPost("/query", Query);
        group.MapPost("/remove", Remove);
        group.MapPost("/updateEdge", SetTagsForEdge);
        group.MapPost("/updateNode", SetTagsForNode);

        return group;
    }

    private async Task<IResult> AddEdge(GraphEdge edge, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!edge.Validate(out var v)) return v.ToResult();

        Option result = await _client.GetDirectoryActor().AddEdge(edge, traceId);
        return result.ToResult();
    }

    private async Task<IResult> AddNode(GraphNode node, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!node.Validate(out var v)) return v.ToResult();

        Option result = await _client.GetDirectoryActor().AddNode(node, traceId);
        return result.ToResult();
    }

    private async Task<IResult> Query(DirectoryQuery search, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option<GraphQueryResult> result = await _client.GetDirectoryActor().Query(search, traceId);
        return result.ToResult();
    }

    private async Task<IResult> Remove(DirectoryQuery search, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option<GraphQueryResult> result = await _client.GetDirectoryActor().Remove(search, traceId);
        return result.ToResult();
    }

    public async Task<IResult> SetTagsForEdge(DirectoryEdgeUpdate model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option result = await _client.GetDirectoryActor().Update(model, traceId);
        return result.ToResult();
    }

    public async Task<IResult> SetTagsForNode(DirectoryNodeUpdate model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option result = await _client.GetDirectoryActor().Update(model, traceId);
        return result.ToResult();
    }
}
