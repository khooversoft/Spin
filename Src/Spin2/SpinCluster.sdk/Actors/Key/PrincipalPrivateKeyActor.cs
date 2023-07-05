using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Key;

public interface IPrincipalPrivateKeyActor : IActionOperation<PrincipalPrivateKeyModel>
{
}


public class PrincipalPrivateKeyActor : ActorDataBase2<PrincipalPrivateKeyModel>, IPrincipalPrivateKeyActor
{
    private readonly IPersistentState<PrincipalPrivateKeyModel> _state;
    private readonly ILogger<PrincipalPrivateKeyActor> _logger;

    public PrincipalPrivateKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.PrincipalKey, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalPrivateKeyModel> state,
        Validator<PrincipalPrivateKeyModel> validator,
        ILogger<PrincipalPrivateKeyActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}