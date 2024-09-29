using Toolbox.TransactionLog;

namespace Toolbox.Graph;

public interface ISelectInstruction
{
    JournalEntry CreateJournal();
}
