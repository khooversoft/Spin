﻿using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.ActorBase;
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
        [PersistentState(stateName: "storageV1", storageName: SpinClusterConstants.SpinStateStore)] IPersistentState<StorageBlob> state,
        Validator<StorageBlob> validator,
        ILogger<StorageActor> logger
        )
        : base(state, validator, logger)
    {
        _state = state;
        _logger = logger;
    }
}