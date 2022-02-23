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
    public static decimal Balance(this BankAccount bankAccount) => bankAccount.Transactions.Sum(x => x.NaturalAmount());
}
