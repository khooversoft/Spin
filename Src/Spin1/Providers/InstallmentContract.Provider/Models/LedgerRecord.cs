using Toolbox.Extensions;
using Toolbox.Tools;

namespace InstallmentContract.Provider.Models;

public sealed record LedgerRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required DateTime Date { get; init; }
    public required LedgerType Type { get; init; }
    public required decimal Amount { get; init; }
    public string? Note { get; init; } = null!;
    public IReadOnlyList<string>? Tags { get; init; }

    public bool Equals(LedgerRecord? obj)
    {
        return obj is LedgerRecord ledgerRecord &&
               Id == ledgerRecord.Id &&
               Date == ledgerRecord.Date &&
               Type == ledgerRecord.Type &&
               Note == ledgerRecord.Note &&
               Amount == ledgerRecord.Amount &&
               (Tags ?? Array.Empty<string>()).SequenceEqual(ledgerRecord.Tags ?? Array.Empty<string>());
    }

    public override int GetHashCode() => HashCode.Combine(Id, Date, Type, Note, Amount, Tags);
}


public static class LedgerExtensions
{
    public static LedgerRecord Verify(this LedgerRecord subject)
    {
        subject.NotNull();

        subject.Note.NotEmpty();
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
