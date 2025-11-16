using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data.Transaction;

public class TransactionScope : IAsyncDisposable
{
    private readonly ActionHandler _commit;
    private readonly ActionHandler _rollback;

    public delegate Task<Option> ActionHandler(ScopeContext context);

    internal TransactionScope(ActionHandler commit, ActionHandler rollback)
    {
        _commit = commit.NotNull();
        _rollback = rollback.NotNull();
    }

    public Task<Option> Commit(ScopeContext context) => _commit(context);

    public Task<Option> Rollback(ScopeContext context) => _rollback(context);

    public async ValueTask DisposeAsync() => await _rollback(NullScopeContext.Instance);
}
