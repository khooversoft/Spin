using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.User;

public interface IUserPrincipleActor : IActorDataBase<UserPrincipal>
{
}


public class UserPrincipleActor : ActorDataBase<UserPrincipal>, IUserPrincipleActor
{
    private readonly IPersistentState<UserPrincipal> _state;
    private readonly ILogger<UserPrincipleActor> _logger;

    public UserPrincipleActor(
        [PersistentState(stateName: SpinConstants.Extension.User, storageName: SpinConstants.SpinStateStore)] IPersistentState<UserPrincipal> state,
        Validator<UserPrincipal> validator,
        ILogger<UserPrincipleActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}
