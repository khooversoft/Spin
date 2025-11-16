using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface ITrxRecorder
{
    void Add<K, T>(K objectId, T newValue) where K : notnull where T : notnull;
    void Delete<K, T>(K objectId, T currentValue) where K : notnull where T : notnull;
    void Update<K, T>(K objectId, T currentValue, T newValue) where K : notnull where T : notnull;
}

public sealed class TrxRecorder(TransactionManager transactionManager, string sourceName) : ITrxRecorder
{
    private readonly string _sourceName = sourceName.NotEmpty();
    private readonly TransactionManager _transactionManager = transactionManager.NotNull();

    public void Add<K, T>(K objectId, T newValue) where K : notnull where T : notnull
    {
        var id = objectId.ToString().NotEmpty();
        var nv = newValue.ToDataETag().WithHash();
        _transactionManager.Enqueue<T>(_sourceName, id, ChangeOperation.Add, null, nv);
    }

    public void Delete<K, T>(K objectId, T currentValue) where K : notnull where T : notnull
    {
        var id = objectId.ToString().NotEmpty();
        var current = currentValue.ToDataETag().WithHash();
        _transactionManager.Enqueue<T>(_sourceName, id, ChangeOperation.Delete, current, null);
    }

    public void Update<K, T>(K objectId, T currentValue, T newValue) where K : notnull where T : notnull
    {
        var id = objectId.ToString().NotEmpty();
        var nv = newValue.ToDataETag().WithHash();
        var current = currentValue.ToDataETag().WithHash();
        _transactionManager.Enqueue<T>(_sourceName, id, ChangeOperation.Update, current, nv);
    }
}
