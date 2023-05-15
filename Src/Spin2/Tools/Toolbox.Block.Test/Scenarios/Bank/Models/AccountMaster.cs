namespace Toolbox.Block.Test.Scenarios.Bank.Models;

public record AccountMaster
{
    public string AccountName { get; init; } = null!;
    public string DocumentId { get; init; } = null!;
    public string OwnerPrincipleId { get; init; } = null!;
    public DateTime? CreatedDate { get; init; } = null!;
}
