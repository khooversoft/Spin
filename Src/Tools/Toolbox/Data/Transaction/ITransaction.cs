using Toolbox.Types;

namespace Toolbox.Data;

public interface ITransaction
{
    string SourceName { get; }
    Task<Option> Begin(ScopeContext context);
    Task<Option> Commit(DataChangeRecord dataChangeEntry, ScopeContext context);
    Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context);
}
