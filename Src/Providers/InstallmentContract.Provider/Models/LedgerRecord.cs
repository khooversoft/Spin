using Toolbox.Extensions;
using Toolbox.Tools;

namespace InstallmentContract.Provider.Models;

public sealed record LedgerRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Date { get; init; }
    public LedgerType Type { get; init; }
    public string TrxType { get; init; } = null!;
    public decimal Amount { get; init; }
    public IReadOnlyList<string>? Properties { get; init; }

    public bool Equals(LedgerRecord? obj)
    {
        return obj is LedgerRecord ledgerRecord &&
               Id == ledgerRecord.Id &&
               Date == ledgerRecord.Date &&
               Type == ledgerRecord.Type &&
               TrxType == ledgerRecord.TrxType &&
               Amount == ledgerRecord.Amount &&
               (Properties ?? Array.Empty<string>()).SequenceEqual(ledgerRecord.Properties ?? Array.Empty<string>());
    }

    public override int GetHashCode() => HashCode.Combine(Id, Date, Type, TrxType, Amount, Properties);
}


public static class LedgerExtensions
{
    public static LedgerRecord Verify(this LedgerRecord subject)
    {
        subject.NotNull();

        subject.TrxType.NotEmpty();
        subject.Type.Assert(x => Enum.IsDefined(typeof(LedgerType), x), "Invalid Ledger type");
        subject.Amount.Assert(x => x > 0, "Amount must be positive");

        return subject;
    }

    public static decimal NaturalAmount(this LedgerRecord ledgerRecord) => ledgerRecord
        .NotNull()
        .Func(x => x.Type switch
        {
            LedgerType.Credit => x.Amount,
            LedgerType.Debit => -x.Amount,
            _ => throw new ArgumentException($"Unknown ledger type={x.Type}"),
        });
}
