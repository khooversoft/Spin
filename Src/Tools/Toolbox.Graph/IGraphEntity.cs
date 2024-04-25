using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphEntity
{
    Task<Option> Delete<T>(T subject, ScopeContext context) where T : class;
    Task<Option<string>> Set<T>(T subject, ScopeContext context) where T : class;
}