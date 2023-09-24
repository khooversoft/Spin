using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class SignatureConnector
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SignatureConnector> _logger;

    public SignatureConnector(
        IClusterClient client,
        ILogger<SignatureConnector> logger)
    {
        _clusterClient = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Signature}");

        group.MapPost("/sign", Sign);
        group.MapPost("/validate", Validate);

        return group;
    }

    private async Task<IResult> Sign(SignRequest model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var result = await _clusterClient
            .GetResourceGrain<ISignatureActor>(SpinConstants.SignValidation)
            .SignDigest(model.PrincipalId, model.MessageDigest, traceId);

        return result.ToResult();
    }

    private async Task<IResult> Validate(SignValidateRequest model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        if (!model.Validate(out Option v)) return Results.BadRequest(v.Error);

        var result = await _clusterClient
            .GetResourceGrain<ISignatureActor>(SpinConstants.SignValidation)
            .ValidateDigest(model.JwtSignature, model.MessageDigest, traceId);

        return result.ToResult();
    }
}