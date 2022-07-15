using Contract.sdk.Client;
using Contract.sdk.Models;
using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public class InstallmentContract
{
    public DateTimeOffset Date { get; init; }
    public Guid ContractId { get; init; } = Guid.NewGuid();
    public string PrincipleId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DocumentId DocumentId { get; init; } = null!;

    public string Issuer { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int NumPayments { get; init; }
    public decimal Principal { get; init; }
    public decimal Payment { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? CompletedDate { get; init; }

    public IReadOnlyList<PartyRecord> Parties { get; init; } = Array.Empty<PartyRecord>();
    public IReadOnlyList<LedgerRecord> Ledger { get; init; } = Array.Empty<LedgerRecord>();
}



public static class InstallmentContractExtensions
{
    public static InstallmentContract Verify(this InstallmentContract subject)
    {
        subject.NotNull();

        subject.PrincipleId.NotEmpty();
        subject.Name.NotEmpty();
        subject.DocumentId.NotNull();
        subject.Parties.NotNull();
        subject.Ledger.NotNull();
        subject.Issuer.NotEmpty();
        subject.Description.NotEmpty();
        subject.NumPayments.Assert(x => x > 0, "NumPayment must be > 0");
        subject.Principal.Assert(x => x > 0.00m, "Principal amount must be > 0");
        subject.Payment.Assert(x => x > 0.00m, "Payment amount must be > 0");

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
