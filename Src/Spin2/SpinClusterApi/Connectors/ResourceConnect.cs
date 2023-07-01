using Microsoft.AspNetCore.Mvc;
using SpinCluster.sdk.Actors.Resource;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using SpinClusterApi.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Connectors;

internal class ResourceConnect
{
    private readonly IClusterClient _client;
    private readonly ILogger<ResourceConnect> _logger;

    public ResourceConnect(IClusterClient client, ILogger<ResourceConnect> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/resource");

        group.MapGet("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) => await Get(objectId, traceId) switch
        {
            var v when v.IsError() => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
            var v when v.HasValue => Results.Ok(v.Return()),
            var v => Results.BadRequest(v.Return()),
        });

        group.MapPost("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId, ResourceFile resourceFile) =>
        {
            StatusCode statusCode = await Set(objectId, traceId, resourceFile);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });

        group.MapDelete("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            StatusCode statusCode = await Delete(objectId, traceId);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });
    }

    public async Task<StatusCode> Delete(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = ApiTools.TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.StatusCode;

        IResourceActor actor = _client.GetGrain<IResourceActor>(objectId);
        return await actor.Delete(context.TraceId);
    }

    public async Task<Option<ResourceFile>> Get(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var id = ApiTools.TestObjectId(objectId, context.Location());
        if (id.IsError()) return id.ToOption<ResourceFile>();

        IResourceActor actor = _client.GetGrain<IResourceActor>(objectId);
        SpinResponse<ResourceFile> response = await actor.Get(context.TraceId);

        if (response.StatusCode.IsError()) return new Option<ResourceFile>(response.StatusCode);
        return response.Return();
    }

    public async Task<StatusCode> Set(string objectId, string traceId, ResourceFile resourceFile)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = ApiTools.TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.StatusCode;

        IResourceActor actor = _client.GetGrain<IResourceActor>(objectId);
        return await actor.Set(resourceFile, context.TraceId);
    }
}
