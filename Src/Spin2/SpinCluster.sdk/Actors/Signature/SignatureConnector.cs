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
using System.Reflection;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class SignatureConnector
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SignatureConnector> _logger;
    private readonly IValidator<SignRequest> _signValidator;
    private readonly IValidator<ValidateRequest> _validateValidator;

    public SignatureConnector(
        IClusterClient client,
        IValidator<SignRequest> signValidator,
        IValidator<ValidateRequest> validateValidator,
        ILogger<SignatureConnector> logger)
    {
        _clusterClient = client.NotNull();
        _signValidator = signValidator.NotNull();
        _validateValidator = validateValidator.NotNull();
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
        var context = new ScopeContext(traceId, _logger);
        var validation = _signValidator.Validate(model).LogResult(context.Location());
        if (!validation.IsValid) return new Option(StatusCode.BadRequest, validation.FormatErrors()).ToResult();

        Option<string> result = await _clusterClient.GetSignatureActor().SignDigest(model.PrincipalId, model.MessageDigest, traceId);
        if( result.IsError()) return result.ToResult();

        var response = new SignResponse
        {
            Kid = model.PrincipalId,
            MessageDigest = model.MessageDigest,
            JwtSignature = result.Return(),
        }.ToOption();

        return response.ToResult();
    }

    private async Task<IResult> Validate(ValidateRequest model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var validation = _validateValidator.Validate(model).LogResult(context.Location());
        if (!validation.IsValid) return new Option(StatusCode.BadRequest, validation.FormatErrors()).ToResult();

        var result = await _clusterClient.GetSignatureActor().ValidateDigest(model.JwtSignature, model.MessageDigest, traceId);
        return result.ToResult();
    }
}