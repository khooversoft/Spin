﻿using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

public interface ISignatureActor : ISign, ISignValidate, IGrainWithStringKey
{
}

[StatelessWorker]
[Reentrant]
public class SignatureActor : Grain, ISignatureActor
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SignatureActor> _logger;

    public SignatureActor(IClusterClient clusterClient, ILogger<SignatureActor> logger)
    {
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task<Option<SignResponse>> SignDigest(string principalId, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("SignDigest, principalId={principalId}", principalId);

        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        string userId = $"{SpinConstants.Schema.User}:{principalId}";

        Option<SignResponse> result = await _clusterClient
            .GetResourceGrain<IUserActor>(userId)
            .SignDigest(messageDigest, traceId);

        if (result.IsError()) return new Option<SignResponse>(result.StatusCode, "Failed to sign messageDigest, " + result.Error);

        return result.Return();
    }

    public async Task<Option> ValidateDigest(string jwtSignature, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("ValidateDigest, jwtSignature={jwtSignature}", jwtSignature);

        string? jwtKid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (jwtKid == null || !IdPatterns.IsKeyId(jwtKid)) return new Option(StatusCode.BadRequest, "no kid in jwtSignature");

        context.Location().LogInformation("Validating signature message digest, kid={kid}", jwtKid);

        Option<ResourceId> resourceIdOption = ResourceId.Create(jwtKid);
        if (resourceIdOption.IsError()) return resourceIdOption.ToOptionStatus();

        ResourceId resourceId = resourceIdOption.Return();
        if (!IdPatterns.IsPrincipalId(resourceId.PrincipalId)) return new Option(StatusCode.BadRequest, "Invalid principal");

        ResourceId publicKeyId = IdTool.CreatePublicKeyId(resourceId.PrincipalId!, resourceId.Path);

        Option response = await _clusterClient.GetResourceGrain<IPrincipalKeyActor>(publicKeyId)
            .ValidateJwtSignature(jwtSignature, messageDigest, context.TraceId);

        if (response.IsError())
        {
            context.Location().LogError("Failed to validated signature, actorKey={actorKey}", this.GetPrimaryKeyString());
            return response;
        }

        context.Location().LogInformation("Digest signature validated, actorKey={actorKey}", this.GetPrimaryKeyString());
        return response;
    }
}
