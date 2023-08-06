using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

internal class PublicKeyAgent
{
    private readonly IPrincipalKeyActor _publicKeyActor;
    public PublicKeyAgent(IPrincipalKeyActor publicKey) => _publicKeyActor = publicKey;

    public async Task<Option> CheckForCreate(ScopeContext context)
    {
        Option publicKeyExist = await _publicKeyActor.Exist(context.TraceId);
        if (publicKeyExist.StatusCode.IsOk())
        {
            const string msg = "Cannot create new Key, public key already exist";
            context.Location().LogError(msg);
            return new Option(StatusCode.BadRequest, msg);
        }

        return new Option(StatusCode.OK);
    }

    public async Task<Option> Delete(ScopeContext context)
    {
        var result = await _publicKeyActor.Delete(context.TraceId);
        return result;
    }

    public Task<Option> Exist(ScopeContext context) => _publicKeyActor.Exist(context.TraceId);

    public async Task<Option> Set(PrincipalKeyModel model, ScopeContext context)
    {
        try
        {
            context.Location().LogInformation("Setting public key for keyId={keyId}", model.KeyId);
            Option result = await _publicKeyActor.Set(model, context.TraceId);
            return result;
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set public actor, keyId={keyId}", model.KeyId);
            return new Option(StatusCode.InternalServerError, $"Failed to set public key actor, keyId={model.KeyId}");
        }
    }

    public async Task<Option> ValidateJwtSignature(string jwtSignature, string digest, ScopeContext context)
    {
        context.Location().LogInformation("Validating JWT signature");

        Option<PrincipalKeyModel> model = await _publicKeyActor.Get(context.TraceId);
        if (model.StatusCode.IsError())
        {
            context.Location().LogError("Public key does not exist to validate digest signature");
            return new Option(model.StatusCode, model.Error);
        }

        var signature = model.Return().ToPrincipalSignature(context);
        if (signature.IsError()) return signature.ToOptionStatus();

        Option validationResult = await signature.Return().ValidateDigest(jwtSignature, digest, context);
        return validationResult;
    }
}

