using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

internal class PrivateKeyAgent
{
    private readonly IPrincipalPrivateKeyActor _privateKeyActor;
    public PrivateKeyAgent(IPrincipalPrivateKeyActor publicKey) => _privateKeyActor = publicKey;

    public async Task<Option> CheckForCreate(ScopeContext context)
    {
        Option publicKeyExist = await _privateKeyActor.Exist(context.TraceId);
        if (publicKeyExist.StatusCode.IsOk())
        {
            const string msg = "Cannot create new Key, private key already exist";
            context.Location().LogError(msg);
            return new Option(StatusCode.BadRequest, msg);
        }

        return new Option(StatusCode.OK);
    }

    public async Task<Option> Delete(ScopeContext context)
    {
        var result = await _privateKeyActor.Delete(context.TraceId);
        return result;
    }

    public Task<Option> Exist(ScopeContext context) => _privateKeyActor.Exist(context.TraceId);

    public async Task<Option> Set(PrincipalPrivateKeyModel model, ScopeContext context)
    {
        try
        {
            context.Location().LogInformation("Setting public key for keyId={keyId}", model.KeyId);
            Option result = await _privateKeyActor.Set(model, context.TraceId);
            return result;
        }
        catch (Exception ex)
        {
            context.Location().LogCritical(ex, "Failed to set public actor, keyId={keyId}", model.KeyId);
            return new Option(StatusCode.InternalServerError, $"Failed to set public key actor, keyId={model.KeyId}");
        }
    }

    public async Task<Option<string>> Sign(string messageDigest, ScopeContext context)
    {
        Option<PrincipalPrivateKeyModel> modelResponse = await _privateKeyActor.Get(context.TraceId);
        if (modelResponse.StatusCode.IsError())
        {
            context.Location().LogError("Priavte key does not exist to create digest signature");
            return new Option<string>(modelResponse.StatusCode, modelResponse.Error);
        }

        PrincipalPrivateKeyModel model = modelResponse.Return();

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(model.PrivateKey, model.KeyId, model.PrincipalId, model.Audience);

        string jwtSignature = new JwtTokenBuilder()
            .SetDigest(messageDigest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(signature)
            .Build();

        if (jwtSignature.IsEmpty())
        {
            context.Location().LogError("Failed to build JWT");
            return new Option<string>(StatusCode.BadRequest, "JWT builder failed");
        }

        return jwtSignature;
    }
}

