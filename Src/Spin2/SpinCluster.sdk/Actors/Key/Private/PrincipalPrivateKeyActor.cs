using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Key.Private;

public interface IPrincipalPrivateKeyActor : IActionOperation<PrincipalPrivateKeyModel>
{
}


public class PrincipalPrivateKeyActor : ActorDataBase2<PrincipalPrivateKeyModel>, IPrincipalPrivateKeyActor
{
    public PrincipalPrivateKeyActor(
        [PersistentState(stateName: SpinConstants.Extension.PrincipalPrivateKey, storageName: SpinConstants.SpinStateStore)] IPersistentState<PrincipalPrivateKeyModel> state,
        IValidator<PrincipalPrivateKeyModel> validator,
        ILogger<PrincipalPrivateKeyActor> logger
        )
        : base(state, validator, logger)
    {
    }
}