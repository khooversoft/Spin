namespace Toolbox.Journal;

public class JournalFileOption
{
    // Connection string format: "journal=/journal/data";
    public string ConnectionString { get; init; } = null!;

    // Writes are performed in the background
    public bool UseBackgroundWriter { get; init; }
}
