using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public partial class Transaction
{
    public async Task<Option> Recovery()
    {
        _runState.IfValue(TrxRunState.None).BeTrue("Cannot recover when transaction is in progress");
        _logger.LogTrace("Recoverying databases");

        var startPoint = GetMinimalLogSequenceNumbers();
        var dataChangeRecords = await GetDataChangeEntries(startPoint);
        await Providers.Recovery(dataChangeRecords);

        _logger.LogTrace("Completed database recovery");
        return StatusCode.OK;
    }

    private Lsn? GetMinimalLogSequenceNumbers()
    {
        Lsn? minLogTime = null;

        foreach (var provider in Providers)
        {
            var providerLsn = provider.GetLogSequenceNumber();
            if (providerLsn.IsEmpty()) return null;

            Lsn logTime = LogSequenceNumber.Parse(providerLsn);
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
