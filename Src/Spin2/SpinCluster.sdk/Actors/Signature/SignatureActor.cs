using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
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

    public async Task<Option<string>> SignDigest(string principalId, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Signing message digest, principalId={principalId}", principalId);

        var userModel = await _clusterClient.GetUserActor(principalId).Get(context.TraceId).LogResult(context.Location());
        if (userModel.IsError()) return userModel.ToOptionStatus<string>();

        ObjectId privateKeyId = userModel.Return().UserKey.PrivateKeyId;

        Option<string> signResponse = await _clusterClient.GetPrivateKeyActor(privateKeyId)
            .Sign(messageDigest, traceId)
            .LogResult(context.Location());

        if (signResponse.IsError())
        {
            context.Location().LogError("Failed to sign, actorKey={actorKey}", this.GetPrimaryKeyString());
            return signResponse;
        }

        context.Location().LogInformation("Digest signed, actorKey={actorKey}", this.GetPrimaryKeyString());
        return signResponse;
    }

    public async Task<Option> ValidateDigest(string jwtSignature, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        string? jwtKid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (jwtKid == null) return new Option(StatusCode.BadRequest, "no kid in jwtSignature");

        context.Location().LogInformation("Validating signature message digest, kid={kid}", jwtKid);

        Option<KeyId> keyId = KeyId.Create(jwtKid);
        if (keyId.IsError()) return keyId.ToOptionStatus();

        var validationResponse = await _clusterClient.GetPublicKeyActor(keyId.Return())
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

