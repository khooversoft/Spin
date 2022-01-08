using Azure;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;

namespace Toolbox.Document
{
    public interface IDocumentPackage
    {
        Task<bool> Delete(DocumentId id, CancellationToken token = default);
        Task<Document?> Get(DocumentId id, CancellationToken token = default);
        Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default);
        Task<ETag> Set(Document document, ETag? eTag = null, CancellationToken token = default);
    }
}