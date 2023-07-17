using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key.Private;

public interface IPrincipalPrivateKeyActor : IActionOperation<PrincipalPrivateKeyModel>
{
    Task<SpinResponse<string>> Sign(string messageDigest, string traceId);
}


public class PrincipalPrivateKeyActor : ActorDataBase2<PrincipalPrivateKeyModel>, IPrincipalPrivateKeyActor
{
    private readonly IPersistentState<PrincipalPrivateKeyModel> _state;
    private readonly IValidator<PrincipalPrivateKeyModel> _validator;
    private readonly ILogger<PrincipalPrivateKeyActor> _logger;

    public PrincipalPrivateKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.PrivateKeyJson, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalPrivateKeyModel> state,
        IValidator<PrincipalPrivateKeyModel> validator,
        ILogger<PrincipalPrivateKeyActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state.NotNull();
        _validator = validator.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalPrivateKey, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<SpinResponse<string>> Sign(string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        await _state.ReadStateAsync();
        if (!_state.RecordExists)
        {
            context.Location().LogError("Private key does not exist to sign digest");
            return new SpinResponse<string>(StatusCode.NotFound);
        }

        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(_state.State.PrivateKey, _state.State.KeyId, _state.State.OwnerId, _state.State.Audience);

        string jwtSignature = new JwtTokenBuilder()
            .SetDigest(messageDigest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(signature)
            .Build();

        if (jwtSignature.IsEmpty())
        {
            context.Location().LogError("Failed to build JWT");
            return new SpinResponse<string>(StatusCode.BadRequest, "JWT builder failed");
        }

        return jwtSignature;
    }
}