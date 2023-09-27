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

        group.MapPost("/", Acquire);
        group.MapGet("/{leaseKey}", Get);
        group.MapGet("/{leaseKey}/isValid", IsValid);
        group.MapPost("/list", List);
        group.MapDelete("/{leaseKey}", Release);

        return group;
    }

    private async Task<IResult> Acquire(LeaseData model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out var v)) return v.ToResult();

        Option response = await _client.GetLeaseActor().Acquire(model, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Get(string leaseKey, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!IdPatterns.IsName(leaseKey)) return Results.BadRequest("Invalid leaseKey");

        Option<LeaseData> response = await _client.GetLeaseActor().Get(leaseKey, traceId);
        return response.ToResult();
    }

    private async Task<IResult> IsValid(string leaseKey, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        leaseKey = Uri.UnescapeDataString(leaseKey);
        if (!IdPatterns.IsName(leaseKey)) return Results.BadRequest("Invalid leaseKey");

        Option response = await _client.GetLeaseActor().IsLeaseValid(leaseKey, traceId);
        return response.ToResult();
    }

    private async Task<IResult> List(QueryParameter query, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        Option<IReadOnlyList<LeaseData>> response = await _client.GetLeaseActor().List(query, traceId);
        return response.ToResult();
    }

    private async Task<IResult> Release(string leaseKey, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        leaseKey = Uri.UnescapeDataString(leaseKey);
        if (!IdPatterns.IsName(leaseKey)) return Results.BadRequest();

        Option response = await _client.GetLeaseActor().Release(leaseKey, traceId);
        return response.ToResult();
    }
}
