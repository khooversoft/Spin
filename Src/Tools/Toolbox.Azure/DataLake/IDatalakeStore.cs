using Azure;
using Toolbox.Types;

namespace Toolbox.Azure;

public interface IDatalakeStore
{
    Task<Option> Append(string path, DataETag data, ScopeContext context);
    Task<Option<IDatalakeLease>> Acquire(string path, TimeSpan leaseDuration, ScopeContext context);
    Task<Option> Delete(string path, ScopeContext context);
    Task<Option> DeleteDirectory(string path, ScopeContext context);
    Task<Option> Exist(string path, ScopeContext context);
    Task<Option<DatalakePathProperties>> GetPathProperties(string path, ScopeContext context);
    Task<Option<DataETag>> Read(string path, ScopeContext context);
    Task<Option<QueryResponse<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context);
    Task<Option<ETag>> Write(string path, DataETag data, bool overwrite, ScopeContext context);
    Task<Option> TestConnection(ScopeContext context);
}

public interface IDatalakeLease : IAsyncDisposable
{
    Task<Option> Release(ScopeContext context);
}

