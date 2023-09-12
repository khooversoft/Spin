using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace SpinCluster.sdk.Actors.Storage;

public class StorageConnection
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<StorageConnection> _logger;

    public StorageConnection(IClusterClient client, ILogger<StorageConnection> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Storage}");

        group.MapDelete("/{storageId}", Delete);
        group.MapGet("/{storageId}/exist", Exist);
        group.MapGet("/{storageId}", Get);
        group.MapPost("/", Set);

        return group;
    }

    private async Task<IResult> Delete(string storageId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        storageId = Uri.UnescapeDataString(storageId);
        if (!ResourceId.IsValid(storageId, ResourceType.DomainOwned)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IStorageActor>(storageId).Delete(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Exist(string storageId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        storageId = Uri.UnescapeDataString(storageId);
        if (!ResourceId.IsValid(storageId, ResourceType.DomainOwned)) return Results.BadRequest();

        Option response = await _client.GetResourceGrain<IStorageActor>(storageId).Exist(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Get(string storageId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        storageId = Uri.UnescapeDataString(storageId);
        if (!ResourceId.IsValid(storageId, ResourceType.DomainOwned)) return Results.BadRequest();

        Option<StorageBlob> response = await _client.GetResourceGrain<IStorageActor>(storageId).Get(traceId);
        return response.ToResult();
    }

    public async Task<IResult> Set(StorageBlob model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var v = model.Validate();
        if (v.IsError()) return Results.BadRequest(v.Error);

        var response = await _client.GetResourceGrain<IStorageActor>(model.StorageId).Set(model, traceId);
        return response.ToResult();
    }
}
