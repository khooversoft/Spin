using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public interface IPrincipalKeyActor : IActionOperation<PrincipalKeyModel>
{
}

public class PrincipalKeyActor : ActorDataBase2<PrincipalKeyModel>, IPrincipalKeyActor
{
    private readonly ILogger<PrincipalKeyModel> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalKeyModel> state,
        IValidator<PrincipalKeyModel> validator,
        ILogger<PrincipalKeyModel> logger
        )
        : base(state, validator, logger)
    {
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalKey, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }
}
