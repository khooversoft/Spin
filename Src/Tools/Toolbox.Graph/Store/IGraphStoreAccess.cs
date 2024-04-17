using Toolbox.Types;

namespace Toolbox.Graph
{
    public interface IGraphStoreAccess
    {
        Task<Option<string>> Add<T>(string nodeKey, string name, T value, ScopeContext context) where T : class;
        Task<Option> Delete(string nodeKey, string name, ScopeContext context);
        Task<Option> Exist(string nodeKey, string name, ScopeContext context);
        Task<Option<T>> Get<T>(string nodeKey, string name, ScopeContext context);
        Task<Option<string>> Set<T>(string nodeKey, string name, T value, ScopeContext context) where T : class;
    }
}