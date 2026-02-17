using Toolbox.Tools;

namespace Toolbox.Data;

public record TrxRecoveryScope
{
    public TrxRecoveryScope(IEnumerable<DataChangeRecord> records, Lsn? logSequenceNumber)
    {
        Records = records as IReadOnlyList<DataChangeRecord> ?? records.ToArray();
        LogSequenceNumber = logSequenceNumber;
    }

    public IReadOnlyList<DataChangeRecord> Records { get; init; }
    public Lsn? LogSequenceNumber { get; set; }
}
