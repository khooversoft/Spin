using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphMapAccess : IAsyncDisposable
{
    bool IsShareMode { get; }
    Task<Option> Start(ScopeContext context);
    Task<Option> Stop(ScopeContext context);
    Task<IGraphMapAccessScope> CreateScope(ScopeContext context);
}

public interface IGraphMapAccessScope : IAsyncDisposable
{
    string LeaseId { get; }
    Task<Option> LoadDatabase(ScopeContext context);
    Task<Option> SaveDatabase(ScopeContext context);
}
