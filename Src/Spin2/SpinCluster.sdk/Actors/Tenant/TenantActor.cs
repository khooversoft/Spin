using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Tenant;

public interface ITenantActor : IActionOperation<TenantModel>
{
}


public class TenantActor : ActorDataBase2<TenantModel>, ITenantActor
{
    private readonly IPersistentState<TenantModel> _state;
    private readonly ILogger<TenantActor> _logger;

    public TenantActor(
        [PersistentState(stateName: SpinConstants.Extension.Tenant, storageName: SpinConstants.SpinStateStore)] IPersistentState<TenantModel> state,
        Validator<TenantModel> validator,
        ILogger<TenantActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}