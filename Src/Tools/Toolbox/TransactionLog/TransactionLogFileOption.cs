namespace Toolbox.TransactionLog;

public record TransactionLogFileOption
{
    public string ConnectionString { get; init; } = "journal=/journal/data";
    public int MaxCount { get; init; } = 1000;
}
