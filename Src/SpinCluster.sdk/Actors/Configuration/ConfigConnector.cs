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

namespace SpinCluster.sdk.Actors.Configuration;

public class ConfigConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<ConfigConnector> _logger;

    public ConfigConnector(IClusterClient client, ILogger<ConfigConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Config}");

        group.MapDelete("/{configId}", Delete);
        group.MapGet("/{configId}/exist", Exist);
        group.MapGet("/{configId}", Get);
        group.MapPost("/removeProperty", RemoveProperty);
        group.MapPost("/", Set);
        group.MapPost("/setProperty", SetProperty);

        return group;
    }

    private async Task<IResult> Delete(string configId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        configId = Uri.UnescapeDataString(configId);
        if (!ResourceId.IsValid(configId, ResourceType.System, SpinConstants.Schema.Config)) return Results.BadRequest("bad configId");

        Option response = await _client.GetResourceGrain<IConfigActor>(configId).Delete(traceId);
        return response.ToResult();
    }

    private async Task<IResult> Exist(string configId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        configId = Uri.UnescapeDataString(configId);
        if (!ResourceId.IsValid(configId, ResourceType.System, SpinConstants.Schema.Config)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IConfigActor>(configId).Exist(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string configId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        configId = Uri.UnescapeDataString(configId);
        if (!ResourceId.IsValid(configId, ResourceType.System, SpinConstants.Schema.Config)) return Results.BadRequest("bad configId");

        Option<ConfigModel> response = await _client.GetResourceGrain<IConfigActor>(configId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> RemoveProperty(RemovePropertyModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<IConfigActor>(model.ConfigId).RemoveProperty(model, traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(ConfigModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<IConfigActor>(model.ConfigId).Set(model, traceId);
        return response.ToResult();
    }

    public async Task<IResult> SetProperty(SetPropertyModel model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<IConfigActor>(model.ConfigId).SetProperty(model, traceId);
        return response.ToResult();
    }
}
