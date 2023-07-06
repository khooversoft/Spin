using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Key.Private;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public interface IPrincipalKeyActor : IActionOperation<PrincipalKeyModel>
{
    Task<SpinResponse> Create(PrincipalKeyRequest request, string traceId);
}

internal class PrincipalKeyActor : ActorDataBase2<PrincipalKeyModel>
{
    private readonly IPersistentState<PrincipalKeyModel> _state;
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.PrincipalKey, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKeyModel> state,
        IValidator<PrincipalKeyModel> validator,
        ILogger<PrincipalKeyActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }

    public async Task<SpinResponse> Create(PrincipalKeyRequest request, IValidator<PrincipalKeyRequest> validator, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var validation = validator.Validate(request, context.Location());
        if (!validation.IsValid) return validation.ToSpinResponse();

        string keyId = this.GetPrimaryKeyString().NotEmpty();
        context.Location().LogInformation("Creating principal key, keyId={keyId}, request={request}", keyId, request);

        await _state.ReadStateAsync();
        if (_state.RecordExists)
        {
            const string msg = "Cannot create new Key, public key already exist";
            context.Location().LogError(msg);
            return new SpinResponse(StatusCode.BadRequest, msg);
        }

        var rsaKey = new RsaKeyPair(keyId);
        IPrincipalPrivateKeyActor privateKeyActor = GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(rsaKey.KeyId);

        if ((await privateKeyActor.Exist(traceId)).StatusCode.IsOk())
        {
            const string msg = "Cannot create new key, private key already exist";
            context.Location().LogError(msg);
            return new SpinResponse(StatusCode.BadRequest, msg);
        }

        var publicKey = new PrincipalKeyModel
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
            _state.State = publicKey;
            await _state.WriteStateAsync();
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
            await _state.ClearStateAsync();
        }

        return new SpinResponse(StatusCode.OK);
    }

    public override async Task<SpinResponse> Delete(string traceId)
    {
        var response = await base.Delete(traceId);
        if (response.StatusCode.IsError()) return response;

        var context = new ScopeContext(traceId, _logger);
        string keyId = this.GetPrimaryKeyString().NotEmpty();

        var privateKeyResponse = await GrainFactory.GetGrain<IPrincipalPrivateKeyActor>(keyId).Delete(traceId);
        if (privateKeyResponse.StatusCode.IsError())
        {
            context.Location().LogWarning("Public key keyId={keyId} does not exist to delete", keyId);
        }

        return response;
    }
}
