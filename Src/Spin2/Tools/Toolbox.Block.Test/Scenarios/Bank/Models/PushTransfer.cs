using Toolbox.Tools;

namespace Toolbox.Block.Test.Scenarios.Bank.Models;

public record PushTransfer
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string ToPath { get; init; } = null!;
    public required string FromPath { get; init; } = null!;
    public required decimal Amount { get; init; }
}


public static class PushTransferExtensions
{
    public static PushTransfer Verify(this PushTransfer subject)
    {
        const string msg = "required";
        subject.NotNull(name: msg);
        subject.ToPath.NotEmpty(name: msg);
        subject.FromPath.NotEmpty(name: msg);

        return subject;
    }
}
