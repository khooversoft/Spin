using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

public class TenantConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<TenantConnector> _logger;

    public TenantConnector(IClusterClient client, ILogger<TenantConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Tenant}");

        group.MapDelete("/{tenantId}", Delete);
        group.MapGet("/{tenantId}/exist", Exist);
        group.MapGet("/{tenantId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string tenantId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        tenantId = Uri.UnescapeDataString(tenantId);
        if (!IdPatterns.IsDomain(tenantId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreateTenantId(tenantId);
        Option response = await _client.GetResourceGrain<ITenantActor>(resourceId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Exist(string tenantId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        tenantId = Uri.UnescapeDataString(tenantId);
        if (!IdPatterns.IsDomain(tenantId)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ITenantActor>(tenantId).Exist(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string tenantId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        tenantId = Uri.UnescapeDataString(tenantId);
        if (!IdPatterns.IsDomain(tenantId)) return Results.BadRequest();

        ResourceId resourceId = IdTool.CreateTenantId(tenantId);
        Option<TenantModel> response = await _client.GetResourceGrain<ITenantActor>(resourceId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(TenantModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<ITenantActor>(model.TenantId).Set(model, traceId);
        return response.ToResult();
    }
}
