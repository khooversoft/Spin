using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public sealed class TrxRecorder
{
    private readonly Transaction _transaction;

    public TrxRecorder(Transaction transaction) => _transaction = transaction.NotNull();

    public TrxSourceRecorder ForSource(string sourceName) => new TrxSourceRecorder(this, sourceName);

    public void Add<K, T>(string sourceName, K objectId, T newValue) where K : notnull where T : notnull
    {
        sourceName.NotEmpty();
        var id = objectId.ToString().NotEmpty();
        var nv = newValue.ToDataETagWithHash();
        _transaction.Enqueue<T>(sourceName, id, ChangeOperation.Add, null, nv);
    }

    public void Delete<K, T>(string sourceName, K objectId, T currentValue) where K : notnull where T : notnull
    {
        sourceName.NotEmpty();
        var id = objectId.ToString().NotEmpty();
        var current = currentValue.ToDataETagWithHash();
        _transaction.Enqueue<T>(sourceName, id, ChangeOperation.Delete, current, null);
    }

    public void Update<K, T>(string sourceName, K objectId, T currentValue, T newValue) where K : notnull where T : notnull
    {
        sourceName.NotEmpty();
        var id = objectId.ToString().NotEmpty();
        var nv = newValue.ToDataETagWithHash();
        var current = currentValue.ToDataETagWithHash();
        _transaction.Enqueue<T>(sourceName, id, ChangeOperation.Update, current, nv);
    }

    public void Checkpoint<T>(string sourceName, T currentValue) where T : notnull
    {
        sourceName.NotEmpty();
        var current = currentValue.ToDataETagWithHash();
        _transaction.Enqueue<T>(sourceName, "checkpoint", ChangeOperation.Checkpoint, current, null);
    }
}
