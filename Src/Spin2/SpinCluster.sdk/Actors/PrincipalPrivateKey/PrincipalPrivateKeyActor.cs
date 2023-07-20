using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalPrivateKey;

public interface IPrincipalPrivateKeyActor : IActionOperation<PrincipalPrivateKeyModel>
{
}

public class PrincipalPrivateKeyActor : ActorDataBase2<PrincipalPrivateKeyModel>, IPrincipalPrivateKeyActor
{
    private readonly ILogger<PrincipalPrivateKeyModel> _logger;

    public PrincipalPrivateKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalPrivateKeyModel> state,
        IValidator<PrincipalPrivateKeyModel> validator,
        ILogger<PrincipalPrivateKeyModel> logger
        )
        : base(state, validator, logger)
    {
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.PrincipalPrivateKey, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }
}
