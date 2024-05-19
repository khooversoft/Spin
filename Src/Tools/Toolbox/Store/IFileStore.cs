using System.Collections.Immutable;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IFileStore
{
    Task<Option<string>> Add(string path, DataETag data, ScopeContext context);
    Task<Option> Delete(string path, ScopeContext context);
    Task<Option> Exist(string path, ScopeContext context);
    Task<ImmutableArray<string>> Search(string pattern, ScopeContext context);
    Task<Option<DataETag>> Get(string path, ScopeContext context);
    Task<Option<string>> Set(string path, DataETag data, ScopeContext context);
}
