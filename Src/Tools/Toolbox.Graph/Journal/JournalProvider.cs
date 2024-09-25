namespace Toolbox.Graph;

public class Journal
{
    private readonly IGraphFileStore _fileStore;

    public Journal(IGraphFileStore fileStore)
    {
        _fileStore = fileStore;
    }
}

public class JournalProvider
{
}

public class JournalWriter
{
}

public enum EntryType
{
    Action,
    StartTran,
    CommitTran,
    RollbackTran,
}

public record JournalEntry
{
    public string TransactionId { get; init; } = Guid.NewGuid().ToString();
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
    public EntryType Type { get; init; }
    public GiNode? GiNode { get; init; }
    public GiEdge? GiEdge { get; init; }
    public GiDelete? GiDelete { get; init; }
    public CmEdgeAdd? CmEdgeAdd { get; init; }
    public CmEdgeChange? CmEdgeChange { get; init; }
    public CmEdgeDelete? CmEdgeDelete { get; init; }
    public CmNodeAdd? CmNodeAdd { get; init; }
    public CmNodeChange? CmNodeChange { get; init; }
    public CmNodeDataDelete? CmNodeDataDelete { get; init; }
    public CmNodeDataSet? CmNodeDataSet { get; init; }
    public CmNodeDelete? CmNodeDelete { get; init; }
}
