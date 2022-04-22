namespace Bank.sdk.Model;

public record BankAccount
{
    public string AccountId { get; init; } = null!;

    public string AccountName { get; init; } = null!;

    public string AccountNumber { get; init; } = null!;

    public IReadOnlyList<TrxRecord> Transactions { get; init; } = Array.Empty<TrxRecord>();

    public IReadOnlyList<TrxRequest> Requests { get; init; } = Array.Empty<TrxRequest>();

    public IReadOnlyList<TrxRequestResponse> Responses { get; init; } = Array.Empty<TrxRequestResponse>();
}


public static class BankAccountExtensions
{
    public static decimal Balance(this BankAccount bankAccount) => bankAccount.Transactions.Sum(x => x.NaturalAmount());
}
