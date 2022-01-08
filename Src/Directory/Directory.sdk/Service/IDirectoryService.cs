using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;

namespace Directory.sdk.Service
{
    public interface IDirectoryService
    {
        Task<bool> Delete(DocumentId documentId, CancellationToken token);
        Task<DirectoryEntry?> Get(DocumentId documentId, CancellationToken token = default, bool bypassCache = false);
        Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default);
        Task<DirectoryEntry> Set(DirectoryEntry entry, CancellationToken token = default);
    }
}