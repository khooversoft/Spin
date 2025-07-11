using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans;

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
    private readonly Func<TState, TSerialize> _toStorageFormat;
    private readonly Func<TSerialize, TState> _fromStorageFormat;
    private readonly CacheObject<TState> _cacheObject = new(TimeSpan.FromMinutes(15));
    private int _firstRead = 0;

    public ActorCacheState(IPersistentState<TSerialize> state, Func<TState, TSerialize> toStorage, Func<TSerialize, TState> fromStorage, TimeSpan? cacheTime = null)
    {
        _state = state.NotNull();
        _toStorageFormat = toStorage.NotNull();
        _fromStorageFormat = fromStorage.NotNull();
    }

    public async Task<Option> Clear()
    {
        if (!_state.RecordExists) return StatusCode.NotFound;
        await _state.ClearStateAsync();
        _cacheObject.Clear();
        return StatusCode.OK;
    }

    public bool RecordExists => _state.RecordExists;
    public TSerialize State => _state.State;
    public string ETag => _state.Etag;
    public Task<Option> Exist() => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public async Task<Option<TState>> GetState()
    {
        if (_cacheObject.TryGetValue(out var value)) return value;
        return await ReadFromStorage();
    }

    public async Task<Option> SetState(TState state)
    {
        _state.State = _toStorageFormat(state);
        await _state.WriteStateAsync();

        _cacheObject.Set(state);
        return StatusCode.OK;
    }

    private async Task<Option<TState>> ReadFromStorage()
    {
        if (!_state.RecordExists) return StatusCode.NotFound;

        if (Interlocked.CompareExchange(ref _firstRead, 1, 0) == 0 && _state.RecordExists)
        {
            TState? f1 = _fromStorageFormat(_state.State);
            _cacheObject.Set(f1);
            return f1;
        }

        await _state.ReadStateAsync();
        _cacheObject.Clear();

        if (!_state.RecordExists) return StatusCode.NotFound;

        TState? f2 = _fromStorageFormat(_state.State);
        _cacheObject.Set(f2);
        return f2;
    }
}