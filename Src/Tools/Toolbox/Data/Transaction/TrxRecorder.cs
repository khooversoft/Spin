using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface ITrxRecorder
{
    void Add<K, T>(string sourceName, K objectId, T newValue) where K : notnull where T : notnull;
    void Delete<K, T>(string sourceName, K objectId, T currentValue) where K : notnull where T : notnull;
    void Update<K, T>(string sourceName, K objectId, T currentValue, T newValue) where K : notnull where T : notnull;
}

public sealed class TrxRecorder : ITrxRecorder
{
    private readonly TransactionManager _transactionManager;
    public TrxRecorder(TransactionManager transactionManager) => _transactionManager = transactionManager.NotNull();

    public void Add<K, T>(string sourceName, K objectId, T newValue) where K : notnull where T : notnull
    {
        sourceName.NotEmpty();
        var id = objectId.ToString().NotEmpty();
        var nv = newValue.ToDataETagWithHash();
        _transactionManager.Enqueue<T>(sourceName, id, ChangeOperation.Add, null, nv);
    }

    public void Delete<K, T>(string sourceName, K objectId, T currentValue) where K : notnull where T : notnull
    {
        sourceName.NotEmpty();
        var id = objectId.ToString().NotEmpty();
        var current = currentValue.ToDataETagWithHash();
        _transactionManager.Enqueue<T>(sourceName, id, ChangeOperation.Delete, current, null);
    }

    public void Update<K, T>(string sourceName, K objectId, T currentValue, T newValue) where K : notnull where T : notnull
    {
        sourceName.NotEmpty();
        var id = objectId.ToString().NotEmpty();
        var nv = newValue.ToDataETagWithHash();
        var current = currentValue.ToDataETagWithHash();
        _transactionManager.Enqueue<T>(sourceName, id, ChangeOperation.Update, current, nv);
    }
}

public sealed class TrxRecorder2 : ITrxRecorder
{
    private readonly Transaction _transaction;
    public TrxRecorder2(Transaction transaction) => _transaction = transaction.NotNull();

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
}
