using Toolbox.Store;
using Toolbox.Types;

namespace Toolbox.Data;

public interface ITrxProvider
{
    public string SourceName { get; }
    void AttachRecorder(TrxRecorder trxRecorder);
    void DetachRecorder();

    public Task<Option> Start();
    public Task<Option> Commit(DataChangeRecord dcr);
    public Task<Option> Rollback(DataChangeEntry dataChangeRecord);
    Task<Option> Recovery(IEnumerable<DataChangeRecord> records);
    Task<Option> Checkpoint();
    Task<Option> Restore(string json);

    void SetLogSequenceNumber(string lsn);
    string? GetLogSequenceNumber();

    Task<string> GetSnapshot();
}

public static class TrxProviderExtensions
{
    public static void AttachRecorder(this IKeyStore keyStore, TrxRecorder trxRecorder)
    {
        ITrxProvider provider = keyStore as ITrxProvider ?? throw new ArgumentException("The IKeyStore is not a ITrxProvider transaction provider");
        provider.AttachRecorder(trxRecorder);
    }

    public static void DetachRecorder(this IKeyStore keyStore)
    {
        ITrxProvider provider = keyStore as ITrxProvider ?? throw new ArgumentException("The IKeyStore is not a ITrxProvider transaction provider");
        provider.DetachRecorder();
    }

    public static void Enlist(this TransactionProviders providers, IKeyStore keyStore)
    {
        ITrxProvider provider = keyStore as ITrxProvider ?? throw new ArgumentException("The IKeyStore is not a ITrxProvider transaction provider");
        providers.Enlist(provider);
    }

    public static void Delist(this TransactionProviders providers, IKeyStore keyStore)
    {
        ITrxProvider provider = keyStore as ITrxProvider ?? throw new ArgumentException("The IKeyStore is not a ITrxProvider transaction provider");
        providers.Delist(provider);
    }
}
