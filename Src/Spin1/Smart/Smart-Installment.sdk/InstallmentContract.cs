using Contract.sdk.Client;
using Contract.sdk.Models;
using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public class InstallmentContract
{
    public DocumentId DocumentId { get; init; } = null!;
    public InstallmentHeader Header { get; init; } = null!;
    public TrxList<PartyRecord> PartyRecords { get; init; } = new TrxList<PartyRecord>();
    public TrxList<LedgerRecord> LedgerRecords { get; init; } = new TrxList<LedgerRecord>();

    public override bool Equals(object? obj)
    {
        return obj is InstallmentContract contract &&
               EqualityComparer<DocumentId>.Default.Equals(DocumentId, contract.DocumentId) &&
               EqualityComparer<TrxList<PartyRecord>>.Default.Equals(PartyRecords, contract.PartyRecords) &&
               EqualityComparer<TrxList<LedgerRecord>>.Default.Equals(LedgerRecords, contract.LedgerRecords);
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, Header, PartyRecords, LedgerRecords);
    public static bool operator ==(InstallmentContract? left, InstallmentContract? right) => EqualityComparer<InstallmentContract>.Default.Equals(left, right);
    public static bool operator !=(InstallmentContract? left, InstallmentContract? right) => !(left == right);
}


public static class InstallmentContractExtensions
{
    public static InstallmentContract Verify(this InstallmentContract subject)
    {
        subject.NotNull();
        subject.Header.Verify();
        subject.PartyRecords.Verify();
        subject.LedgerRecords.Verify();

        return subject;
    }

    public static decimal Balance(this IEnumerable<LedgerRecord> ledgerRecords)
    {
        ledgerRecords.NotNull();

        return ledgerRecords
            .Select(x => x.NaturalAmount())
            .Sum(x => x);
    }
}
