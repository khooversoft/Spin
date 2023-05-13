using Toolbox.Tools;

namespace Toolbox.Block.Test.Scenarios.Bank.Models;

public record ApplyDeposit
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string ToPath { get; init; } = null!;
    public required string FromPath { get; init; } = null!;
    public required decimal Amount { get; init; }
}


public static class ApplyDepositExtensions
{
    public static ApplyDeposit Verify(this ApplyDeposit subject)
    {
        const string msg = "required";
        subject.NotNull(name: msg);

        return subject;
    }
}