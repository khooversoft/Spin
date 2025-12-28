using Toolbox.Tools;

namespace Toolbox.Data;

public class TrxSourceRecorder
{
    private readonly TrxRecorder _recorder;
    private readonly string _sourceName;

    public TrxSourceRecorder(TrxRecorder transaction, string sourceName)
    {
        _recorder = transaction.NotNull();
        _sourceName = sourceName.NotEmpty();
    }

    public void Add<K, T>(K objectId, T newValue) where K : notnull where T : notnull =>
        _recorder.Add<K, T>(_sourceName, objectId, newValue);

    public void Delete<K, T>(K objectId, T currentValue) where K : notnull where T : notnull =>
        _recorder.Delete<K, T>(_sourceName, objectId, currentValue);

    public void Update<K, T>(K objectId, T currentValue, T newValue) where K : notnull where T : notnull =>
        _recorder.Update<K, T>(_sourceName, objectId, currentValue, newValue);
}