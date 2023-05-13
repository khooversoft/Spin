namespace Toolbox.Block.Test.Scenarios.Bank.Models;

public enum LedgerType
{
    Credit,
    Debit
}


public static class LedgerTypeExtensions
{
    public static decimal NaturalAmount(this LedgerType type, decimal amount) => type switch
    {
        LedgerType.Credit => Math.Abs(amount),
        LedgerType.Debit => -Math.Abs(amount),

        _ => throw new ArgumentException($"Invalid type={type}")
    };
}