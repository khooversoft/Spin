using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Key;

public interface IPrincipalPublicKeyActor : IActionOperation<PrincipalPublicKeyModel>
{
}


public class PrincipalPublicKeyActor : ActorDataBase2<PrincipalPublicKeyModel>, IPrincipalPublicKeyActor
{
    private readonly IPersistentState<PrincipalPublicKeyModel> _state;
    private readonly ILogger<PrincipalPublicKeyActor> _logger;

    public PrincipalPublicKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.PrincipalKey, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalPublicKeyModel> state,
        Validator<PrincipalPublicKeyModel> validator,
        ILogger<PrincipalPublicKeyActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}