using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Directory.sdk.Models;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Directory.sdk.Actors;

public abstract class ActorBase<T> : Grain
{
    private readonly IPersistentState<T> _state;
    private readonly ILogger _logger;

    public ActorBase(IPersistentState<T> state, ILogger logger)
    {
        _state = state;
        _logger = logger;
    }

    public async Task

    public async Task<Option<T>> Get()
    {
        _logger.LogInformation("Getting {typeName}, id={id}", typeof(T).GetTypeName(), this.GetPrimaryKeyString());

        await _state.ReadStateAsync();

        if (!_state.RecordExists) return Option<T>.None;
        return _state.State;
    }
}
