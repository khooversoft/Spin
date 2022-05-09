using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public record LedgerRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTimeOffset Date { get; init; }
    public LedgerType Type { get; init; }
    public string TrxType { get; init; } = null!;
    public decimal Amount { get; init; }
    public IReadOnlyList<string>? Properties { get; init; }
}


public static class LedgerExtensions
{
    public static LedgerRecord Verify(this LedgerRecord subject)
    {
        subject.NotNull(nameof(subject));

        subject.Id.NotEmpty(nameof(subject.Id));
        subject.TrxType.NotEmpty(nameof(subject.TrxType));
        subject.Type.Assert(x => Enum.IsDefined(typeof(LedgerType), x), "Invalid Ledger type");
        subject.Amount.Assert(x => x > 0, "Amount must be positive");

        return subject;
    }

    public static decimal NaturalAmount(this LedgerRecord ledgerRecord) => ledgerRecord
        .NotNull(nameof(ledgerRecord))
        .Func(x => x.Type switch
        {
            LedgerType.Credit => x.Amount,
            LedgerType.Debit => -x.Amount,
            _ => throw new ArgumentException($"Unknown ledger type={x.Type}"),
        });
}
