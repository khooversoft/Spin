using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.ActorBase;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Directory;

public interface IPrincipalKeyActor : IActorDataBase<PrincipalKey>
{
}


public class PrincipalKeyActor : ActorDataBase<PrincipalKey>, IPrincipalKeyActor
{
    private readonly IPersistentState<PrincipalKey> _state;
    private readonly ILogger<PrincipalKeyActor> _logger;

    public PrincipalKeyActor(
        [PersistentState(stateName: "principalKeyV1", storageName: "principalKey")] IPersistentState<PrincipalKey> state,
        Validator<PrincipalKey> validator,
        ILogger<PrincipalKeyActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}