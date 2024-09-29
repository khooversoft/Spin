namespace Toolbox.TransactionLog;

public record TransactionLogFileOption
{
    public string ConnectionString { get; init; } = null!;
    public int MaxCount { get; init; } = 1000;
}


