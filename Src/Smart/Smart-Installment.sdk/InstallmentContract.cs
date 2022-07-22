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
    public IReadOnlyList<PartyRecord> CommittedParties { get; init; } = Array.Empty<PartyRecord>();
    public IList<PartyRecord> Parties { get; init; } = new List<PartyRecord>();
    public IReadOnlyList<LedgerRecord> CommittedLedger { get; init; } = Array.Empty<LedgerRecord>();
    public IList<LedgerRecord> Ledger { get; init; } = new List<LedgerRecord>();

    public override bool Equals(object? obj)
    {
        return obj is InstallmentContract contract &&
               EqualityComparer<DocumentId>.Default.Equals(DocumentId, contract.DocumentId) &&
               EqualityComparer<InstallmentHeader>.Default.Equals(Header, contract.Header) &&

               CommittedParties.Count == contract.CommittedParties.Count &&
               Parties.Count == contract.Parties.Count &&
               CommittedLedger.Count == contract.CommittedLedger.Count &&
               Ledger.Count == contract.Ledger.Count &&

               Enumerable.SequenceEqual(CommittedParties, contract.CommittedParties) &&
               Enumerable.SequenceEqual(Parties, contract.Parties) &&
               Enumerable.SequenceEqual(CommittedLedger, contract.CommittedLedger) &&
               Enumerable.SequenceEqual(Ledger, contract.Ledger);
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, Header, CommittedParties, Parties, CommittedLedger, Ledger);
    public static bool operator ==(InstallmentContract? left, InstallmentContract? right) => EqualityComparer<InstallmentContract>.Default.Equals(left, right);
    public static bool operator !=(InstallmentContract? left, InstallmentContract? right) => !(left == right);
}


public static class InstallmentContractExtensions
{
    public static InstallmentContract Verify(this InstallmentContract subject)
    {
        subject.NotNull();
        subject.Header.Verify();
        subject.CommittedParties.NotNull();
        subject.Parties.NotNull();
        subject.Ledger.NotNull();
        subject.CommittedLedger.NotNull();

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
