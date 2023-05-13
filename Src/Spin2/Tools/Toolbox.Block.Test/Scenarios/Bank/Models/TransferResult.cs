using Toolbox.Types;

namespace Toolbox.Block.Test.Scenarios.Bank.Models;

public record TransferResult
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public StatusCode Status { get; init; }
    public string Description { get; init; } = null!;

    public static TransferResult Ok() => new TransferResult { Status = StatusCode.OK };
    public static TransferResult Error() => new TransferResult { Status = StatusCode.BadRequest };
}
