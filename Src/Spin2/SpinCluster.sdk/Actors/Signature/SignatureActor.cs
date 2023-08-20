using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

public interface ISignatureActor : ISignValidate, IGrainWithStringKey
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

    public async Task<Option> ValidateDigest(string jwtSignature, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        string? jwtKid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (jwtKid == null || !IdPatterns.IsKeyId(jwtKid)) return new Option(StatusCode.BadRequest, "no kid in jwtSignature");

        context.Location().LogInformation("Validating signature message digest, kid={kid}", jwtKid);

        Option<ResourceId> resourceIdOption = ResourceId.Create(jwtKid);
        if (resourceIdOption.IsError()) return resourceIdOption.ToOptionStatus();

        ResourceId resourceId = resourceIdOption.Return();
        if (!IdPatterns.IsPrincipalId(resourceId.PrincipalId)) return new Option(StatusCode.BadRequest, "Invalid principal");

        ResourceId publicKeyId = IdTool.CreatePublicKeyId(resourceId.PrincipalId!, resourceId.Path);

        Option response = await _clusterClient.GetPublicKeyActor(publicKeyId)
            .ValidateJwtSignature(jwtSignature, messageDigest, context.TraceId)
            .LogResult(context.Location());

        if (response.IsError())
        {
            context.Location().LogError("Failed to validated signature, actorKey={actorKey}", this.GetPrimaryKeyString());
            return response;
        }

        context.Location().LogInformation("Digest signature validated, actorKey={actorKey}", this.GetPrimaryKeyString());
        return response;
    }
}

