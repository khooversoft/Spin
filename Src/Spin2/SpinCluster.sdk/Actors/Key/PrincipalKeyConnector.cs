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
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public class PrincipalKeyConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<PrincipalKeyConnector> _logger;

    public PrincipalKeyConnector(IClusterClient client, ILogger<PrincipalKeyConnector> logger)
    {
        _client = client;
        _logger = logger;
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"/{SpinConstants.ApiPath.PrincipalKey}");

        group.MapGet("/{*keyId}", async (string keyId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var response = await Get(keyId, traceId);

            return response switch
            {
                var v when v.StatusCode.IsOk() => Results.Ok(v.Return()),
                var v when v.StatusCode == StatusCode.NotFound => Results.NotFound(v.ToStatusResponse()),
                var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
            };
        });

        group.MapPost("/", async ([FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId, PrincipalKeyRequest model) =>
        {
            var response = await Create(model, traceId);
            return constructResponse(response);
        });

        group.MapDelete("/{*keyId}", async (string keyId, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var response = await Delete(keyId, traceId);
            return constructResponse(response);
        });

        IResult constructResponse(StatusResponse response) => response switch
        {
            var v when v.StatusCode.IsOk() => Results.Ok(),
            var v when v.StatusCode == StatusCode.NotFound => Results.NotFound(v.Error),
            var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
        };
    }

    public async Task<Option<PrincipalKeyModel>> Get(string keyId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        IPrincipalKeyActor actor = _client.GetGrain<IPrincipalKeyActor>(keyId);
        SpinResponse<PrincipalKeyModel> response = await actor.Get(context.TraceId);
        return response.ToOption();
    }

    public async Task<StatusResponse> Create(PrincipalKeyRequest model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var validation = model.Validate(context.Location());
        if (!validation.IsValid) return validation.ToStatusResponse();

        IPrincipalKeyActor actor = _client.GetGrain<IPrincipalKeyActor>(model.KeyId);
        var response = await actor.Create(model, context.TraceId);
        return response.ToStatusResponse();
    }

    public async Task<StatusResponse> Delete(string keyId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        IPrincipalKeyActor actor = _client.GetGrain<IPrincipalKeyActor>(keyId);
        var response = await actor.Delete(context.TraceId);
        return response.ToStatusResponse();
    }
}
