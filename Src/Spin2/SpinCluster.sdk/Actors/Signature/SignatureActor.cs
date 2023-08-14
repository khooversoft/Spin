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

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Signature, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<string>> SignDigest(string kid, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Signing message digest, kid={kid}", kid);

        Option<string> signResponse = await _clusterClient.GetObjectGrain<IPrincipalPrivateKeyActor>(kid)
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

        string? kid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (kid == null) return new Option(StatusCode.BadRequest, "no kid in jwtSignature");

        context.Location().LogInformation("Validating signature message digest, kid={kid}", kid);

        string privateKeyId = ObjectId.Create(kid).Return()
            .WithSchema(SpinConstants.Schema.PrincipalPrivateKey)
            .ToString();

        var validationResponse = await _clusterClient.GetObjectGrain<IPrincipalKeyActor>(privateKeyId)
            .ValidateJwtSignature(jwtSignature, messageDigest, context);

        if (validationResponse.IsError())
        {
            context.Location().LogError("Failed to validated signature, actorKey={actorKey}", this.GetPrimaryKeyString());
            return validationResponse;
        }

        context.Location().LogInformation("Digest signature validated, actorKey={actorKey}", this.GetPrimaryKeyString());
        return validationResponse;
    }
}

