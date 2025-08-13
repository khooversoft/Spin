using Toolbox.Types;

namespace Toolbox.Store;

public interface IKeyStore<T>
{
    IKeyStore<T>? InnerHandler { get; set; }

    Task<Option> Append(string key, T value, ScopeContext context);
    Task<Option> Delete(string key, ScopeContext context);
    Task<Option<T>> Get(string key, ScopeContext context);
    Task<Option<string>> Set(string key, T value, ScopeContext context);

    Task<Option> AcquireLock(string key, ScopeContext context);
    Task<Option> AcquireExclusiveLock(string key, ScopeContext context);
    Task<Option> ReleaseLock(string key, ScopeContext context);
}
