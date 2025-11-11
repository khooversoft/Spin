using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public sealed class TrxRecorder(TransactionManager transactionManager, string sourceName)
{
    private readonly string _sourceName = sourceName.NotEmpty();
    private readonly TransactionManager _transactionManager = transactionManager.NotNull();

    public void Add<K, T>(K objectId, T newValue) where K : notnull where T : notnull
    {
        _transactionManager.Enqueue<T>(_sourceName, objectId.ToString().NotEmpty(), ChangeOperation.Add, null, newValue.ToDataETag());
    }

    public void Delete<K, T>(K objectId, T currentValue) where K : notnull where T : notnull
    {
        _transactionManager.Enqueue<T>(_sourceName, objectId.ToString().NotEmpty(), ChangeOperation.Delete, currentValue.ToDataETag(), null);
    }

    public void Update<K, T>(K objectId, T currentValue, T newValue) where K : notnull where T : notnull
    {
        _transactionManager.Enqueue<T>(_sourceName, objectId.ToString().NotEmpty(), ChangeOperation.Update, currentValue.ToDataETag(), newValue.ToDataETag());
    }
}
