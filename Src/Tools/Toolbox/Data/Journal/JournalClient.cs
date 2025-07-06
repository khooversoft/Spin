using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IJournalClient
{
    IJournalTrx CreateTransaction();
    Task<Option<IReadOnlyList<JournalEntry>>> GetList(ScopeContext context);
    Task<Option> DeleteList(ScopeContext context);
    Task<Option> Drain(ScopeContext context);
}


public class JournalClient : IJournalClient
{
    private readonly IDataClient<JournalEntry> _dataClient;
    private readonly string _pipelineName;
    private readonly IServiceProvider _serviceProvider;
    private readonly LogSequenceNumber _logSequence;

    public JournalClient(IDataClient<JournalEntry> dataClient, string pipelineName, LogSequenceNumber logSequence, IServiceProvider serviceProvider)
    {
        _dataClient = dataClient.NotNull();
        _pipelineName = pipelineName.NotEmpty();
        _serviceProvider = serviceProvider.NotNull();
        _logSequence = logSequence.NotNull();
    }

    public IJournalTrx CreateTransaction()
    {
        var trxId = Guid.NewGuid().ToString();

        IJournalTrx trx = new JournalTrx(_dataClient, _logSequence, _pipelineName, trxId, _serviceProvider.GetRequiredService<ILogger<JournalTrx>>());
        return trx;
    }

    public Task<Option<IReadOnlyList<JournalEntry>>> GetList(ScopeContext context) => _dataClient.GetList(_pipelineName, context);
    public Task<Option> DeleteList(ScopeContext context) => _dataClient.DeleteList(_pipelineName, context);
    public Task<Option> Drain(ScopeContext context) => _dataClient.Drain(context);
}
