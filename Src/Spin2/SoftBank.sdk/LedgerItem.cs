using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftBank.sdk;

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

public record LedgerItem
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Description { get; init; } = null!;
    public required LedgerType Type { get; init; }
    public required decimal Amount { get; init; }

    public decimal NaturalAmount => Type.NaturalAmount(Amount);
}