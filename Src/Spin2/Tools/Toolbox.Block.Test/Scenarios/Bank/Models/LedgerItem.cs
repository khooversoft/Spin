namespace Toolbox.Block.Test.Scenarios.Bank.Models;

public record LedgerItem
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Description { get; init; } = null!;
    public required LedgerType Type { get; init; }
    public required decimal Amount { get; init; }

    public decimal NaturalAmount => Type.NaturalAmount(Amount);
}