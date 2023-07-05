using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.User;

public interface IUserActor : IActionOperation<UserModel>
{
}


public class UserActor : ActorDataBase2<UserModel>, IUserActor
{
    private readonly IPersistentState<UserModel> _state;
    private readonly ILogger<UserActor> _logger;

    public UserActor(
        [PersistentState(stateName: SpinConstants.Extension.User, storageName: SpinConstants.SpinStateStore)] IPersistentState<UserModel> state,
        IValidator<UserModel> validator,
        ILogger<UserActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}
