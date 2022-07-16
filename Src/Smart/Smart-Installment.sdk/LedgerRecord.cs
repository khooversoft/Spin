using Toolbox.Block.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public record LedgerRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
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

    public static DataItem ConvertTo(this LedgerRecord subject)
    {
        subject.NotNull();

        return new DataItem
        {
            Id = subject.Id,
            Key = subject.GetType().Name,
            Value = subject.ToJson(),
        };
    }

    public static LedgerRecord ConvertTo(this DataItem dataItem) => dataItem
        .NotNull()
        .Assert(x => x.Key == typeof(LedgerRecord).Name, "Invalid type")
        .Value
        .ToObject<LedgerRecord>(true)!;
}
