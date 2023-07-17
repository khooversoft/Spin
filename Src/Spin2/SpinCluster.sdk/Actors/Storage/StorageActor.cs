using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Storage;

public interface IStorageActor : IActorDataBase<StorageBlob>
{
}


public class StorageActor : ActorDataBase<StorageBlob>, IStorageActor
{
    private readonly IPersistentState<StorageBlob> _state;
    private readonly ILogger<StorageActor> _logger;

    public StorageActor(
        [PersistentState(stateName: SpinConstants.Extension.Json, storageName: SpinConstants.SpinStateStore)] IPersistentState<StorageBlob> state,
        IValidator<StorageBlob> validator,
        ILogger<StorageActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}
