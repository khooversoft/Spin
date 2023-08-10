using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

public interface ISignatureActor : IGrainWithStringKey
{
    Task<Option> Create(PrincipalKeyRequest request, string traceId);
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> ValidateJwtSignature(string jwtSignature, string digest, string traceId);
    Task<Option<string>> Sign(string messageDigest, string traceId);
}

public class SignatureActor : Grain, ISignatureActor
{
    private readonly IValidator<PrincipalKeyRequest> _validator;
    private readonly ILogger<SignatureActor> _logger;
    private PublicKeyAgent _publicKeyAgent = null!;
    private PrivateKeyAgent _privateKeyAgent = null!;

    public SignatureActor(IValidator<PrincipalKeyRequest> validator, ILogger<SignatureActor> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalKey, new ScopeContext(_logger));

        IPrincipalKeyActor publicKey = GrainFactory.GetGrain<IPrincipalKeyActor>(this.GetPrimaryKeyString());
        _publicKeyAgent = new PublicKeyAgent(publicKey);

        string privateKeyActorId = ObjectId.Create(this.GetPrimaryKeyString()).Return()
            .WithSchema(SpinConstants.Schema.PrincipalPrivateKey)
            .ToString();

        IPrincipalPrivateKeyActor privateKey = GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(privateKeyActorId);
        _privateKeyAgent = new PrivateKeyAgent(privateKey);

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Create(PrincipalKeyRequest request, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating principal key, primarykey={primaryKey}, request={request}", this.GetPrimaryKeyString(), request);

        var validation = _validator.Validate(request, context.Location());
        if (!validation.IsValid) return validation.ToOption();

        if (request.KeyId != this.GetPrimaryKeyString())
        {
            return new Option(StatusCode.BadRequest, $"requst.KeyId={request.KeyId} does not match actor key={this.GetPrimaryKeyString()}");
        }

        var publicKeyExist = await _publicKeyAgent.CheckForCreate(context);
        if (publicKeyExist.StatusCode.IsError()) return publicKeyExist;

        Option privateKeyExist = await _privateKeyAgent.CheckForCreate(context);
        if (privateKeyExist.StatusCode.IsError()) return privateKeyExist;

        var rsaKey = new RsaKeyPair(request.KeyId);

        var publicKey = new PrincipalKeyModel
        {
            KeyId = rsaKey.KeyId,
            OwnerId = request.OwnerId,
            Name = request.Name,
            Audience = request.Audience,
            PublicKey = rsaKey.PublicKey,
            PrivateKeyExist = true,
        };

        Option writePublicKeyResult = await _publicKeyAgent.Set(publicKey, context);
        if (writePublicKeyResult.StatusCode.IsError()) return writePublicKeyResult;

        var privateKey = new PrincipalPrivateKeyModel
        {
            KeyId = rsaKey.KeyId,
            OwnerId = request.OwnerId,
            Name = request.Name,
            Audience = request.Audience,
            PrivateKey = rsaKey.PrivateKey
        };

        var writePrivateKeyResult = await _privateKeyAgent.Set(privateKey, context);
        if (writePrivateKeyResult.StatusCode.IsError()) return writePrivateKeyResult;

        return new Option(StatusCode.OK);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        await _publicKeyAgent.Delete(context);
        await _privateKeyAgent.Delete(context);

        return new Option(StatusCode.OK);
    }

    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option publicExist = await _publicKeyAgent.Exist(context);
        Option privateExist = await _privateKeyAgent.Exist(context);

        return (publicExist.StatusCode, privateExist.StatusCode) switch
        {
            (StatusCode.OK, StatusCode.OK) => new Option(StatusCode.OK),
            _ => new Option(StatusCode.Conflict),
        };
    }

    public async Task<Option> ValidateJwtSignature(string jwtSignature, string digest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option response = await _publicKeyAgent.ValidateJwtSignature(jwtSignature, digest, context);
        if (response.StatusCode.IsError())
        {
            context.Location().LogError("JwtSignature is invalid for {actorId}, error={error}", this.GetPrimaryKeyString(), response.Error);
        }

        return response;
    }

    public async Task<Option<string>> Sign(string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<string> response = await _privateKeyAgent.Sign(messageDigest, context);
        if (response.StatusCode.IsError())
        {
            context.Location().LogError("Cannot sign messageDigest for {actorId}, error={error}", this.GetPrimaryKeyString(), response.Error);
        }

        return response;
    }
}

