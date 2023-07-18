using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Types;
using SpinCluster.sdk.Actors.PrincipalKey;

namespace SpinCluster.sdk.Actors.Signature;

internal class PublicKeyAgent
{
    private readonly IPrincipalKeyActor _publicKeyActor;
    public PublicKeyAgent(IPrincipalKeyActor publicKey) => _publicKeyActor = publicKey;

    public async Task<SpinResponse> CheckForCreate(ScopeContext context)
    {
        SpinResponse publicKeyExist = await _publicKeyActor.Exist(context.TraceId);
        if (publicKeyExist.StatusCode.IsOk())
        {
            const string msg = "Cannot create new Key, public key already exist";
            context.Location().LogError(msg);
            return new SpinResponse(StatusCode.BadRequest, msg);
        }

        return new SpinResponse(StatusCode.OK);
    }

    public async Task<SpinResponse> Delete(ScopeContext context)
    {
        var result = await _publicKeyActor.Delete(context.TraceId);
        return result;
    }

    public Task<SpinResponse> Exist(ScopeContext context) => _publicKeyActor.Exist(context.TraceId);

    public async Task<SpinResponse> Set(PrincipalKeyModel model, ScopeContext context)
    {
        try
        {
            context.Location().LogInformation("Setting public key for keyId={keyId}", model.KeyId);
            SpinResponse result = await _publicKeyActor.Set(model, context.TraceId);
            return result;
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set public actor, keyId={keyId}", model.KeyId);
            return new SpinResponse(StatusCode.InternalServerError, $"Failed to set public key actor, keyId={model.KeyId}");
        }
    }

    public async Task<SpinResponse> ValidateJwtSignature(string jwtSignature, string digest, ScopeContext context)
    {
        context.Location().LogInformation("Validating JWT signature");

        SpinResponse<PrincipalKeyModel> model = await _publicKeyActor.Get(context.TraceId);
        if (model.StatusCode.IsError())
        {
            context.Location().LogError("Public key does not exist to validate digest signature");
            return new SpinResponse(model.StatusCode, model.Error);
        }

        var signature = model.Return().ToPrincipalSignature(context);
        if (signature.IsError()) return signature.ToSpinResponse();

        Option<JwtTokenDetails> validationResult = await signature.Return().ValidateDigest(jwtSignature, digest, context);
        return validationResult.ToSpinResponse();
    }
}

