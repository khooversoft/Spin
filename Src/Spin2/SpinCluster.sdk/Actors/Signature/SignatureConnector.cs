using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
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

        group.MapPost("/", Validate);

        return group;
    }

    private async Task<IResult> Validate(ValidateRequest model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var validation = model.Validate().LogResult(context.Location());
        if (validation.IsError()) return validation.ToResult();

        var result = await _clusterClient.GetSignatureActor().ValidateDigest(model.JwtSignature, model.MessageDigest, traceId);
        return result.ToResult();
    }
}