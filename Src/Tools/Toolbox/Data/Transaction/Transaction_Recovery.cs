using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public partial class Transaction
{
    public async Task<Option> Recovery()
    {
        _runState.TryMove(TrxRunState.None, TrxRunState.Recovery).BeTrue("Transaction is not in progress");
        _logger.LogTrace("Recoverying databases");

        var startPoint = GetMinimalLogSequenceNumbers();
        var dataChangeRecords = await GetDataChangeEntries(startPoint);

        var scope = new TrxRecoveryScope(dataChangeRecords, startPoint);
        await Providers.Recovery(scope);

        _runState.TryMove(TrxRunState.Recovery, TrxRunState.None).BeTrue("Transaction is not in finalized");
        _logger.LogTrace("Completed database recovery");
        return StatusCode.OK;
    }

    private Lsn? GetMinimalLogSequenceNumbers()
    {
        Lsn? minLogTime = null;

        foreach (var provider in Providers)
        {
            var providerLsn = provider.GetLogSequenceNumber();
            if (providerLsn.IsError()) continue;

            Lsn logTime = LogSequenceNumber.Parse(providerLsn.Return());
            if (minLogTime is null || logTime < minLogTime)
            {
                minLogTime = logTime;
            }
        }

        if (minLogTime == null) return null;
        return minLogTime.Value;
    }

    private async Task<IReadOnlyList<DataChangeRecord>> GetDataChangeEntries(Lsn? startPoint)
    {
        IReadOnlyList<DataChangeRecord> records = startPoint switch
        {
            null => (await _changeClient.Get(_trxOption.JournalKey)).BeOk().Return(),
            Lsn sp => (await _changeClient.GetHistory(_trxOption.JournalKey, sp.TimestampDate.AddSeconds(-1))).BeOk().Return()
        };

        return records;
    }
}
