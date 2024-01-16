using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ActorCacheState<T> : ActorCacheState<T, T>
{
    public ActorCacheState(IPersistentState<T> state, TimeSpan? cacheTime = null)
        : base(state, x => x, x => x, cacheTime)
    {
    }
}

public class ActorCacheState<TState, TSerialize>
{
    private readonly IPersistentState<TSerialize> _state;
    private readonly Func<TState, TSerialize> _toStorage;
    private readonly Func<TSerialize, TState> _fromStorage;
    private CacheObject<TState> _cacheState = new CacheObject<TState>(TimeSpan.FromMinutes(15));
    private int _firstRead = 0;

    public ActorCacheState(IPersistentState<TSerialize> state, Func<TState, TSerialize> toStorage, Func<TSerialize, TState> fromStorage, TimeSpan? cacheTime = null)
    {
        _state = state.NotNull();
        _toStorage = toStorage.NotNull();
        _fromStorage = fromStorage.NotNull();
        _cacheState = new CacheObject<TState>(cacheTime ?? TimeSpan.FromMinutes(15));
    }

    public async Task<Option> Clear()
    {
        if (!_state.RecordExists) return StatusCode.NotFound;
        await _state.ClearStateAsync();
        _cacheState.Clear();
        return StatusCode.OK;
    }

    public bool RecordExists => _state.RecordExists;
    public Task<Option> Exist() => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public async Task<Option<TState>> GetState()
    {
        if (_cacheState.TryGetValue(out var state)) return state;

        return await ReadFromStorage(true);
    }

    private async Task<Option<TState>> ReadFromStorage(bool forceRead = false)
    {
        if (!_state.RecordExists) return StatusCode.NotFound;

        if (Interlocked.CompareExchange(ref _firstRead, 1, 0) == 0 && _state.RecordExists)
        {
            _cacheState.Set(_fromStorage(_state.State));
            return _cacheState.Value;
        }

        if (forceRead) await _state.ReadStateAsync();
        _cacheState.Clear();

        if (!_state.RecordExists) return StatusCode.NotFound;

        _cacheState.Set(_fromStorage(_state.State));
        return _cacheState.Value;
    }

    public async Task<Option> SetState(TState state)
    {
        _state.State = _toStorage(state);

        await _state.WriteStateAsync();
        _cacheState.Set(state);

        return StatusCode.OK;
    }
}