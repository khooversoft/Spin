using Azure;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;

namespace Toolbox.Document
{
    public interface IDocumentStorage
    {
        Task Delete(DocumentId documentId, CancellationToken token = default);
        Task<(T?, ETag? eTag)> Get<T>(DocumentId documentId, CancellationToken token = default, bool bypassCache = false);
        Task<DatalakePathProperties> GetProperty(DocumentId documentId, CancellationToken token = default);
        Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default);
        Task<ETag> Set<T>(DocumentId documentId, T value, ETag? eTag = null, CancellationToken token = default);
    }
}