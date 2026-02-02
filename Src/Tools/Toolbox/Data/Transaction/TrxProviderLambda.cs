using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TrxProviderLambda : ITrxProvider
{
    private readonly string _sourceName;
    private readonly Func<DataChangeEntry, Task<Option>> _rollback;

    public TrxProviderLambda(string sourceName, Func<DataChangeEntry, Task<Option>> rollback)
    {
        _sourceName = sourceName.NotEmpty();
        _rollback = rollback.NotNull();
    }

    public string SourceName => _sourceName;

    public void AttachRecorder(TrxRecorder trxRecorder) { }
    public void DetachRecorder() { }
    public Task<Option> Start() => new Option(StatusCode.OK).ToTaskResult();
    public Task<Option> Commit() => new Option(StatusCode.OK).ToTaskResult();

    public Task<Option> Rollback(DataChangeEntry dataChangeRecord) => _rollback(dataChangeRecord);
}

public static class TrxProviderLambdaExtensions
{
    public static void EnlistLambda(this Transaction transaction, string sourceName, Func<DataChangeEntry, Task<Option>> rollback)
    {
        transaction.NotNull();
        sourceName.NotEmpty();
        rollback.NotNull();

        var provider = new TrxProviderLambda(sourceName, rollback);
        transaction.Providers.Enlist(provider);
    }
}