using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

public interface ITenantActor : IActionOperation<TenantModel>
{
}


public class TenantActor : ActorDataBase2<TenantModel>, ITenantActor
{
    private readonly IPersistentState<TenantModel> _state;
    private readonly ILogger<TenantActor> _logger;

    public TenantActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<TenantModel> state,
        IValidator<TenantModel> validator,
        ILogger<TenantActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Tenant, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }
}