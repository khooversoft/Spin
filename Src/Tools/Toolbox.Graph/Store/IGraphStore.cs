using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphStore
{
    Task<Option> Add<T>(string nodeKey, T node, ScopeContext context) where T : class;
    Task<Option> Exist(string nodeKey, ScopeContext context);
    Task<Option> Delete(string nodeKey, ScopeContext context);
    Task<Option<T>> Get<T>(string nodeKey, ScopeContext context);
    Task<Option> Set<T>(string nodeKey, T node, ScopeContext context) where T : class;
}
