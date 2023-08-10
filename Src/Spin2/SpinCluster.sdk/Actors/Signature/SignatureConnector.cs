using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Application;
using Toolbox.Orleans.Types;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class SignatureConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<SignatureConnector> _logger;

    public SignatureConnector(IClusterClient client, ILogger<SignatureConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Signature}");

        group.MapDelete(_logger, async (objectId, context) => await _client.GetGrain<ISignatureActor>(objectId).Delete(context.TraceId));
        group.MapExist(_logger, async (objectId, context) => await _client.GetGrain<ISignatureActor>(objectId).Exist(context.TraceId));

        // Create
        group.MapPost("/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId, PrincipalKeyRequest model) =>
        {
            var context = new ScopeContext(traceId, _logger);
            Option<ObjectId> option = ObjectId.Create(objectId).LogResult(context.Location());
            if (option.IsError()) option.ToResult();

            Option response = await _client.GetGrain<ISignatureActor>(objectId).Create(model, traceId);
            return response.ToResult();
        });

        // Sign
        group.MapPost("/sign/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId, SignRequest model) =>
        {
            var context = new ScopeContext(traceId, _logger);
            Option<ObjectId> option = ObjectId.Create(objectId).LogResult(context.Location());
            if (option.IsError()) option.ToResult();

            Option<string> response = await _client.GetGrain<ISignatureActor>(objectId).Sign(model.Digest, context.TraceId);
            return response.ToResult();
        });

        // Validate
        group.MapPost("/validate/{*objectId}", async (string objectId, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId, ValidateRequest model) =>
        {
            var context = new ScopeContext(traceId, _logger);
            Option<ObjectId> option = ObjectId.Create(objectId).LogResult(context.Location());
            if (option.IsError()) option.ToResult();

            Option response = await _client.GetGrain<ISignatureActor>(objectId).ValidateJwtSignature(model.JwtSignature, model.Digest, context.TraceId);
            return response.ToResult();
        });

        return group;
    }
}