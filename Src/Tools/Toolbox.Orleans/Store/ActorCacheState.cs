using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

public class ActorCacheState<T> : ActorCacheState<T, T>
{
    public ActorCacheState(StateManagement stateManagement, IPersistentState<T> state, TimeSpan? cacheTime = null)
        : base(stateManagement, state, x => x, x => x, cacheTime)
    {
    }
}

public class ActorCacheState<TState, TSerialize>
{
    private readonly IPersistentState<TSerialize> _state;
    private readonly Func<TState, TSerialize> _toStorage;
    private readonly Func<TSerialize, TState> _fromStorage;
    private readonly StateManagement _stateManagement;
    private string? _name;
    private int _firstRead = 0;

    public ActorCacheState(StateManagement stateManagement, IPersistentState<TSerialize> state, Func<TState, TSerialize> toStorage, Func<TSerialize, TState> fromStorage, TimeSpan? cacheTime = null)
    {
        _stateManagement = stateManagement.NotNull();
        _state = state.NotNull();
        _toStorage = toStorage.NotNull();
        _fromStorage = fromStorage.NotNull();
    }

    public void SetName(string actorName, string keyName) => _name = actorName.NotEmpty() + "::" + keyName.NotEmpty();

    public async Task<Option> Clear()
    {
        _name.NotEmpty("Name not set");

        if (!_state.RecordExists) return StatusCode.NotFound;
        await _state.ClearStateAsync();
        _stateManagement.Clear(_name);
        return StatusCode.OK;
    }

    public bool RecordExists => _state.RecordExists;
    public TSerialize State => _state.State;
    public Task<Option> Exist() => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public async Task<Option<TState>> GetState(ScopeContext context)
    {
        _name.NotEmpty("Name not set");

        var cacheState = _stateManagement.Get<TState>(_name, context);
        if (cacheState.IsOk()) return cacheState;

        return await ReadFromStorage(context);
    }

    public async Task<Option> SetState(TState state, ScopeContext context)
    {
        _name.NotEmpty("Name not set");

        _state.State = _toStorage(state);
        await _state.WriteStateAsync();

        _stateManagement.Set(_name, state, context);
        return StatusCode.OK;
    }

    private async Task<Option<TState>> ReadFromStorage(ScopeContext context)
    {
        _name.NotEmpty("Name not set");

        if (!_state.RecordExists) return StatusCode.NotFound;

        if (Interlocked.CompareExchange(ref _firstRead, 1, 0) == 0 && _state.RecordExists)
        {
            TState? f1 = _fromStorage(_state.State);
            _stateManagement.Set(_name, f1, context);
            return f1;
        }

        await _state.ReadStateAsync();
        _stateManagement.Clear(_name);

        if (!_state.RecordExists) return StatusCode.NotFound;

        TState? f2 = _fromStorage(_state.State);
        _stateManagement.Set(_name, f2, context);
        return f2;
    }
}