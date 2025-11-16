using Toolbox.Tools;

namespace Toolbox.Data;

public sealed class DataChangeRecorder
{
    private ITrxRecorder? _trxRecorder;
    private ITrxRecorder? _pausedRecorder;

    public ITrxRecorder? GetRecorder() => _trxRecorder;
    public void Set(ITrxRecorder trxRecorder) => Interlocked.CompareExchange(ref _trxRecorder, trxRecorder, null).Assert(x => x == null, "Recorder already set");
    public void Clear() => Interlocked.Exchange(ref _trxRecorder, null);
    public void Pause(bool pause)
    {
        _ = pause switch
        {
            true => Interlocked.Exchange(ref _pausedRecorder, Interlocked.Exchange(ref _trxRecorder, null)).Assert(x => x != null, "No recorder to pause"),
            false => Interlocked.Exchange(ref _trxRecorder, Interlocked.Exchange(ref _pausedRecorder, null)).Assert(x => x != null, "No recorder to resume"),
        };
    }
}