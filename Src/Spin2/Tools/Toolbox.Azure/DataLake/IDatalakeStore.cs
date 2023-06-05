using Azure;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Azure.DataLake
{
    public interface IDatalakeStore
    {
        Task<StatusCode> Append(string path, byte[] data, ScopeContext context);
        Task<StatusCode> Delete(string path, ScopeContext context);
        Task<StatusCode> DeleteDirectory(string path, ScopeContext context);
        Task<StatusCode> Exist(string path, ScopeContext context);
        Task<Option<DatalakePathProperties>> GetPathProperties(string path, ScopeContext context);
        Task<Option<DataETag>> Read(string path, ScopeContext context);
        Task<Option<IReadOnlyList<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context);
        Task<Option<ETag>> Write(string path, DataETag data, bool overwrite, ScopeContext context);
    }
}