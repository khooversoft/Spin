using Toolbox.Types;

namespace Toolbox.Data;

public interface ITransactionProvider
{
    public string Name { get; }
    public Task<Option> Prepare(DataChangeRecord dataChangeEntry, ScopeContext context);
    public Task<Option> Commit(DataChangeRecord dataChangeEntry, ScopeContext context);
    public Task<Option> Rollback(DataChangeEntry dataChangeEntry, ScopeContext context);
}
