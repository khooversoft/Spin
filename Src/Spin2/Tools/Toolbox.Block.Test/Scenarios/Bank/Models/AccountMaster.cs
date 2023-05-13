namespace Toolbox.Block.Test.Scenarios.Bank.Models;

public record AccountMaster
{
    public string AccountName { get; init; } = null!;
    public string DocumentId { get; init; } = null!;
    public string OwnerPrincipleId { get; init; } = null!;
    public DateTime? UpdateDate { get; init; } = null!;
    public int Counter { get; init; }
    public string? Message { get; init; } = null!;
}
