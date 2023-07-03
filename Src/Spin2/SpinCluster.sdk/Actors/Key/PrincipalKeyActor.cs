using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Types;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public interface IPrincipalKeyActor : IGrainWithStringKey
{
    Task<SpinResponse> Create(PrincipalKeyRequest request, string traceId);
    Task<SpinResponse> Delete(string traceId);
    Task<SpinResponse<PrincipalKeyModel>> Get(string traceId);
}

internal class PrincipalKeyActor : Grain
{
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(ILogger<PrincipalKeyActor> logger) => _logger = logger;

    public async Task<SpinResponse> Create(PrincipalKeyRequest request, IValidator<PrincipalKeyRequest> validator, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var validation = validator.Validate(request, context.Location());
        if(!validation.IsValid) return validation.ToSpinResponse();

        string keyId = this.GetPrimaryKeyString().NotEmpty();
        context.Location().LogInformation("Creating principal key, keyId={keyId}, request={request}", keyId, request);

        var rsaKey = new RsaKeyPair(keyId);
        IPrincipalPublicKeyActor publicKeyActor = GrainFactory.GetGrain<IPrincipalPublicKeyActor>(rsaKey.KeyId);
        IPrincipalPrivateKeyActor privateKeyActor = GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(rsaKey.KeyId);

        bool publicExist = (await publicKeyActor.Exist(traceId)).StatusCode.IsOk();
        bool privateExist = (await privateKeyActor.Exist(traceId)).StatusCode.IsOk();

        if (publicExist || privateExist)
        {
            context.Location().LogError("Private and/or public key already exist, publicKeyExist={publicKeyExist}, privateKeyExist={privateKeyExist}", publicExist, privateExist);
            return new SpinResponse(StatusCode.BadRequest, "public and/or private key exist");
        }

        var publicKey = new PrincipalPublicKeyModel
        {
            KeyId = rsaKey.KeyId,
            OwnerId = request.OwnerId,
            Name = request.Name,
            PublicKey = rsaKey.PublicKey
        };

        var privateKey = new PrincipalPrivateKeyModel
        {
            KeyId = rsaKey.KeyId,
            OwnerId = request.OwnerId,
            Name = request.Name,
            PrivateKey = rsaKey.PrivateKey
        };

        try
        {
            context.Location().LogInformation("Setting public key for keyId={keyId}", rsaKey.KeyId);
            await publicKeyActor.Set(publicKey, traceId);
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set public actor, trying to reverting, keyId={keyId}", rsaKey.KeyId);
            return new SpinResponse(StatusCode.InternalServerError, $"Failed to set public key actor, keyId={keyId}");
        }

        try
        {
            context.Location().LogInformation("Setting private key for keyId={keyId}", rsaKey.KeyId);
            await privateKeyActor.Set(privateKey, traceId);
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set private actor, trying to reverting, keyId={keyId}", rsaKey.KeyId);
            await publicKeyActor.Delete(traceId);
        }

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        string keyId = this.GetPrimaryKeyString().NotEmpty();

        context.Location().LogInformation("Getting keyId={keyId}", keyId);

        IPrincipalPublicKeyActor publicKeyActor = GrainFactory.GetGrain<IPrincipalPublicKeyActor>(keyId);
        IPrincipalPrivateKeyActor privateKeyActor = GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(keyId);

        var publicKeyResponse = await publicKeyActor.Delete(traceId);
        if (publicKeyResponse.StatusCode.IsError())
        {
            context.Location().LogWarning("Public key keyId={keyId} does not exist to delete", keyId);
        }

        var privateKeyResponse = await privateKeyActor.Delete(traceId);
        if (publicKeyResponse.StatusCode.IsError())
        {
            context.Location().LogWarning("Public key keyId={keyId} does not exist to delete", keyId);
        }

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse<PrincipalKeyModel>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        string keyId = this.GetPrimaryKeyString().NotEmpty();

        context.Location().LogInformation("Getting keyId={keyId}", keyId);

        IPrincipalPublicKeyActor publicKeyActor = GrainFactory.GetGrain<IPrincipalPublicKeyActor>(keyId);
        IPrincipalPrivateKeyActor privateKeyActor = GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(keyId);

        var publicKeyResponse = await publicKeyActor.Get(traceId);
        if (publicKeyResponse.StatusCode.IsError())
        {
            context.Location().LogError("Public key keyId={keyId} does not exist", keyId);
            return new SpinResponse<PrincipalKeyModel>(StatusCode.NotFound, "Public key does not exist");
        }

        var privateKeyResponse = await privateKeyActor.Get(traceId);

        var response = new PrincipalKeyModel
        {
            KeyId = keyId,
            OwnerId = publicKeyResponse.Return().OwnerId,
            Name = publicKeyResponse.Return().Name,
            PrivateKeyExist = privateKeyResponse.StatusCode.IsOk(),
        };

        return response.ToSpinResponse();
    }
}
