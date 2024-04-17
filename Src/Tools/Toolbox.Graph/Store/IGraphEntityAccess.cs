using Toolbox.Types;

namespace Toolbox.Graph
{
    public interface IGraphEntityAccess
    {
        Task<Option> Delete<T>(T subject, ScopeContext context) where T : class;
        Task<Option<string>> Set<T>(T subject, ScopeContext context) where T : class;
    }
}