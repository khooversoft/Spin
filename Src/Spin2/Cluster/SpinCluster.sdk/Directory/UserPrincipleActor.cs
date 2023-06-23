using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.ActorBase;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Directory;

public interface IUserPrincipleActor : IActorDataBase<UserPrincipal>
{
}


public class UserPrincipleActor : ActorDataBase<UserPrincipal>, IUserPrincipleActor
{
    private readonly IPersistentState<UserPrincipal> _state;
    private readonly ILogger<UserPrincipleActor> _logger;

    public UserPrincipleActor(
        [PersistentState(stateName: "userPrincipalV1", storageName: "user")] IPersistentState<UserPrincipal> state,
        Validator<UserPrincipal> validator,
        ILogger<UserPrincipleActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}
