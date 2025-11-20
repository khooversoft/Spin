using Toolbox.Tools;

namespace Toolbox.Data;

public sealed class DataChangeRecorder
{
    private const int _off = 0;
    private const int _on = 1;
    private const int _paused = 2;
    private int _state = _off;

    private ITrxRecorder? _trxRecorder;

    public ITrxRecorder? GetRecorder()
    {
        // Fast path: only return recorder when fully "on"
        if (Volatile.Read(ref _state) != _on) return null;
        return Volatile.Read(ref _trxRecorder);
    }

    public void Set(ITrxRecorder trxRecorder)
    {
        trxRecorder.NotNull();

        // Set recorder first, then publish state
        Interlocked.CompareExchange(ref _trxRecorder, trxRecorder, null).Assert(x => x == null, "Recorder already set");
        Interlocked.Exchange(ref _state, _on);
    }

    public void Clear()
    {
        // Turn off first so readers do not observe _on while clearing
        Interlocked.Exchange(ref _state, _off);
        Interlocked.Exchange(ref _trxRecorder, null);
    }

    // Idempotent: multiple calls to Pause do not throw
    public void Pause()
    {
        while (true)
        {
            var state = Volatile.Read(ref _state);
            if (state == _off || state == _paused) return;          // nothing to do
            if (Interlocked.CompareExchange(ref _state, _paused, _on) == _on) return;
            // retry if raced
        }
    }

    // Idempotent: multiple calls to Resume do not throw
    public void Resume()
    {
        while (true)
        {
            var state = Volatile.Read(ref _state);
            if (state == _off || state == _on) return;              // nothing to do
            if (Interlocked.CompareExchange(ref _state, _on, _paused) == _paused) return;
            // retry if raced
        }
    }

    public bool IsOn => Volatile.Read(ref _state) == _on;
    public bool IsPaused => Volatile.Read(ref _state) == _paused;
}