namespace Bank.sdk.Model;

public record BankAccount
{
    public string AccountId { get; init; } = null!;

    public string AccountName { get; init; } = null!;

    public string AccountNumber { get; init; } = null!;

    public IReadOnlyList<TrxRecord> Transactions { get; init; } = new List<TrxRecord>();
}


public static class BankAccountExtensions
{
    public static decimal Balance(this BankAccount bankAccount) => bankAccount.Transactions
        .Select(x => x.Type switch
        {
            TrxType.Credit => x.Amount,
            TrxType.Debit => - x.Amount,

            _ => throw new ArgumentException($"Unknown type={x.Type}"),
        })
        .Sum();
}
