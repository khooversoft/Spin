using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Tenant;

public interface ITenantActorActor : IActorDataBase<TenantRegister>
{
}


public class TenantActor : ActorDataBase<TenantRegister>, ITenantActorActor
{
    private readonly IPersistentState<TenantRegister> _state;
    private readonly ILogger<TenantActor> _logger;

    public TenantActor(
        [PersistentState(stateName: SpinConstants.Extension.Tenant, storageName: SpinConstants.SpinStateStore)] IPersistentState<TenantRegister> state,
        Validator<TenantRegister> validator,
        ILogger<TenantActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}