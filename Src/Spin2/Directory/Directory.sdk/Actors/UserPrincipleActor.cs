using Directory.sdk.Models;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Tools.Validation;

namespace Directory.sdk.Actors;

public interface IUserPrincipleActor : IActorDataBase<UserPrincipal>
{
}


public class UserPrincipleActor : ActorDataBase<UserPrincipal>, IUserPrincipleActor
{
    private readonly IPersistentState<UserPrincipal> _state;
    private readonly ILogger<UserPrincipleActor> _logger;

    public UserPrincipleActor(
        [PersistentState(stateName: "Customer", storageName: "Customers")] IPersistentState<UserPrincipal> state,
        Validator<UserPrincipal> validator,
        ILogger<UserPrincipleActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}
