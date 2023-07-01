using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Key;

public interface IPrincipalKeyActor : IActorDataBase<PrincipalKey>
{
}


public class PrincipalKeyActor : ActorDataBase<PrincipalKey>, IPrincipalKeyActor
{
    private readonly IPersistentState<PrincipalKey> _state;
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Key, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKey> state,
        Validator<PrincipalKey> validator,
        ILogger<PrincipalKeyActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}