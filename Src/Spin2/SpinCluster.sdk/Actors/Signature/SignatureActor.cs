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
        if (jwtKid == null) return new Option(StatusCode.BadRequest, "no kid in jwtSignature");

        context.Location().LogInformation("Validating signature message digest, kid={kid}", jwtKid);

        Option<KeyId> keyId = KeyId.Create(jwtKid);
        if (keyId.IsError()) return keyId.ToOptionStatus();

        var validationResponse = await _clusterClient.GetPublicKeyActor(keyId.Return().ToString())
            .ValidateJwtSignature(jwtSignature, messageDigest, context.TraceId);

        if (validationResponse.IsError())
        {
            context.Location().LogError("Failed to validated signature, actorKey={actorKey}", this.GetPrimaryKeyString());
            return validationResponse;
        }

        context.Location().LogInformation("Digest signature validated, actorKey={actorKey}", this.GetPrimaryKeyString());
        return validationResponse;
    }
}

