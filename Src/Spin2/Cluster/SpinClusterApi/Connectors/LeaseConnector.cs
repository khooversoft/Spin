using Microsoft.AspNetCore.Mvc;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using SpinClusterApi.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Connectors;

internal class LeaseConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<LeaseConnector> _logger;

    public LeaseConnector(IClusterClient client, ILogger<LeaseConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/lease");

        group.MapGet("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) => await Acquire(objectId, traceId) switch
        {
            var v when v.IsError() => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
            var v when v.HasValue => Results.Ok(v.Return()),
            var v => Results.BadRequest(v.Return()),
        });

        group.MapDelete("/{leaseId}/{*objectId}", async (string leaseId, string objectId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            StatusCode statusCode = await Release(objectId, leaseId, traceId);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });
    }

    public async Task<Option<LeaseData>> Acquire(ObjectId objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = ApiTools.TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.ToOption<LeaseData>();

        ILeaseActor actor = _client.GetGrain<ILeaseActor>(objectId);
        SpinResponse<LeaseData> response = await actor.Acquire(context.TraceId);

        if (response.StatusCode.IsError()) return new Option<LeaseData>(response.StatusCode);
        return response.Return();
    }

    public async Task<StatusCode> Release(ObjectId objectId, string leaseId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var oObjectId = ApiTools.TestObjectId(objectId, context.Location());
        if (oObjectId.IsError()) return oObjectId.StatusCode;

        ILeaseActor actor = _client.GetGrain<ILeaseActor>(objectId);
        return await actor.Release(leaseId, traceId);
    }
}
