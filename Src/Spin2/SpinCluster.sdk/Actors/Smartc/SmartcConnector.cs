using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
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

        group.MapDelete("/{nameId}", Delete);
        group.MapGet("/{nameId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if( !ResourceId.IsValid(nameId, ResourceType.DomainOwned, "smartc") ) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<ISmartcActor>(nameId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string nameId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        nameId = Uri.UnescapeDataString(nameId);
        if( !ResourceId.IsValid(nameId, ResourceType.DomainOwned, "smartc") ) return Results.BadRequest();

        Option<SmartcModel> response = await _client.GetResourceGrain<ISmartcActor>(nameId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(SmartcModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate();
        if (v.IsError()) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<ISmartcActor>(model.SmartcId).Set(model, traceId);
        return response.ToResult();
    }
}
