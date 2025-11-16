using Toolbox.Tools;

namespace Toolbox.Data;

public sealed class DataChangeRecorder
{
    private ITrxRecorder? _trxRecorder;
    public ITrxRecorder? GetRecorder() => _trxRecorder;
    public void Set(ITrxRecorder trxRecorder) => Interlocked.CompareExchange(ref _trxRecorder, trxRecorder, null).Assert(x => x == null, "Recorder already set");
    public void Clear() => Interlocked.Exchange(ref _trxRecorder, null);
}