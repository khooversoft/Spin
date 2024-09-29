using Toolbox.TransactionLog;

namespace Toolbox.Graph;

public interface IGraphInstruction
{
    IReadOnlyList<JournalEntry> CreateJournals();
}
