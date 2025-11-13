using Toolbox.Tools;

namespace Toolbox.Data;

public sealed class DataChangeRecorder
{
    private TrxRecorder? _trxRecorder;
    public TrxRecorder? GetRecorder() => _trxRecorder;
    public void Set(TrxRecorder trxRecorder) => Interlocked.CompareExchange(ref _trxRecorder, trxRecorder, null).Assert(x => x == null, "Recorder already set");
    public void Clear() => Interlocked.Exchange(ref _trxRecorder, null);
}