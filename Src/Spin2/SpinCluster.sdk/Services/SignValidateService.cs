using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Key.Private;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

internal class SignValidateService : ISign, ISignValidate
{
    private readonly ILogger<SignValidateService> _logger;
    private readonly IClusterClient _client;

    public SignValidateService(IClusterClient client, ILogger<SignValidateService> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<string>> SignDigest(string kid, string messageDigest, ScopeContext context)
    {
        context = context.With(_logger);
        context.Location().LogInformation("Signing kid={kid}", kid);

        IPrincipalPrivateKeyActor privateKeyActor = _client.GetGrain<IPrincipalPrivateKeyActor>(kid);
        SpinResponse<string> result = await privateKeyActor.Sign(messageDigest, context.TraceId);

        return result.ToOption<string>().LogResult(context.Location());
    }

    public async Task<Option<JwtTokenDetails>> ValidateDigest(string jwtSignature, string messageDigest, ScopeContext context)
    {
        context = context.With(_logger);
        string? kid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (kid == null) return new Option<JwtTokenDetails>(StatusCode.BadRequest, "no kid in jwtSignature");

        IPrincipalKeyActor principalKeyActor = _client.GetGrain<IPrincipalKeyActor>(kid);
        SpinResponse result = await principalKeyActor.ValidateJwtSignature(jwtSignature, messageDigest, context.TraceId);

        return result.ToOption<JwtTokenDetails>().LogResult(context.Location());
    }
}
