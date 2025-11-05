using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public readonly struct TransactionRecorder
{
    private readonly string _sourceName;
    private readonly TransactionManager _transactionManager;

    public TransactionRecorder(TransactionManager transactionManager, string sourceName)
    {
        _transactionManager = transactionManager.NotNull();
        _sourceName = sourceName.NotEmpty();
    }

    public void Add<T>(string objectId, T newValue) => _transactionManager.Enqueue<T>(_sourceName, objectId, ChangeOperation.Add, null, newValue.ToDataETag());
    public void Delete<T>(string objectId, T currentValue) => _transactionManager.Enqueue<T>(_sourceName, objectId, ChangeOperation.Delete, currentValue.ToDataETag(), null);
    public void Update<T>(string objectId, T currentValue, T newValue) =>
        _transactionManager.Enqueue<T>(_sourceName, objectId, ChangeOperation.Update, currentValue.ToDataETag(), newValue.ToDataETag());
}
