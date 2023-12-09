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

namespace SpinCluster.sdk.Actors.Smartc;

public class SmartcConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<SmartcConnector> _logger;

    public SmartcConnector(IClusterClient client, ILogger<SmartcConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Smartc}");

        group.MapDelete("/{smartcId}", Delete);
        group.MapGet("/{smartcId}/exist", Exist);
        group.MapGet("/{smartcId}", Get);
        group.MapPost("/", Set);
        //group.MapPost("/{smartcId}/setPayload", SetPayload);

        return group;
    }

    private async Task<IResult> Delete(string smartcId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        smartcId = Uri.UnescapeDataString(smartcId);
        if (!ResourceId.IsValid(smartcId, ResourceType.DomainOwned, SpinConstants.Schema.Smartc)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISmartcActor>(smartcId).Delete(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string smartcId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        smartcId = Uri.UnescapeDataString(smartcId);
        if (!ResourceId.IsValid(smartcId, ResourceType.DomainOwned, SpinConstants.Schema.Smartc)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISmartcActor>(smartcId).Exist(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Get(string smartcId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        smartcId = Uri.UnescapeDataString(smartcId);
        if (!ResourceId.IsValid(smartcId, ResourceType.DomainOwned, SpinConstants.Schema.Smartc)) return Results.BadRequest();

        Option<SmartcModel> response = await _client.GetResourceGrain<ISmartcActor>(smartcId).Get(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Set(SmartcModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<ISmartcActor>(model.SmartcId).Set(model, traceId);
        return response.ToResult();
    }

    //private async Task<IResult> SetPayload(string smartcId, DataObject model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    //{
    //    smartcId = Uri.UnescapeDataString(smartcId);
    //    if (!ResourceId.IsValid(smartcId, ResourceType.DomainOwned, SpinConstants.Schema.Smartc)) return Results.BadRequest();
    //    if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

    //    var response = await _client.GetResourceGrain<ISmartcActor>(smartcId).SetPayload(model, traceId);
    //    return response.ToResult();
    //}
}
