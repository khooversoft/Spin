using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

public class LeaseConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<LeaseConnector> _logger;

    public LeaseConnector(IClusterClient client, ILogger<LeaseConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Lease}");

        group.MapPost("/{leaseId}", Acquire);
        group.MapDelete("/{leaseId}/{leaseKey}", Release);
        group.MapGet("/{leaseId}/isValid/{leaseKey}", IsValid);
        group.MapGet("/{leaseId}/list", List);

        return group;
    }

    private async Task<IResult> Acquire(string leaseId, LeaseCreate model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        leaseId = Uri.UnescapeDataString(leaseId);
        if (!IdPatterns.IsLeaseId(leaseId)) return Results.BadRequest();

        Option<LeaseData> response = await _client.GetResourceGrain<ILeaseActor>(leaseId).Acquire(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Release(string leaseId, string leaseKey, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        leaseId = Uri.UnescapeDataString(leaseId);
        leaseKey = Uri.UnescapeDataString(leaseKey);

        var test = new OptionTest()
            .Test(() => IdPatterns.IsLeaseId(leaseId))
            .Test(() => IdPatterns.IsName(leaseKey));
        if (test.IsError()) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ILeaseActor>(leaseId).Release(leaseKey, traceId);
        return response.ToResult();
    }

    private async Task<IResult> IsValid(string leaseId, string leaseKey, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        leaseId = Uri.UnescapeDataString(leaseId);
        leaseKey = Uri.UnescapeDataString(leaseKey);

        var test = new OptionTest()
            .Test(() => IdPatterns.IsLeaseId(leaseId))
            .Test(() => IdPatterns.IsName(leaseKey));
        if (test.IsError()) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ILeaseActor>(leaseId).IsLeaseValid(leaseKey, traceId);
        return response.ToResult();
    }

    private async Task<IResult> List(string leaseId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        leaseId = Uri.UnescapeDataString(leaseId);
        if (!IdPatterns.IsLeaseId(leaseId)) return Results.BadRequest();

        Option<IReadOnlyList<LeaseData>> response = await _client.GetResourceGrain<ILeaseActor>(leaseId).List(traceId);
        return response.ToResult();
    }
}
