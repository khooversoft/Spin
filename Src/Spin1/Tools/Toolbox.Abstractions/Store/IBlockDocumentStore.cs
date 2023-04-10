using Toolbox.Models;
using Toolbox.Protocol;

namespace Toolbox.Store;

public interface IBlockDocumentStore
{
    Task<bool> Exists(DocumentId id, CancellationToken token);
    Task<bool> Delete(DocumentId id, CancellationToken token);
    Task<Document?> Get(DocumentId id, CancellationToken token);
    Task<IReadOnlyList<StorePathItem>> Search(QueryParameter queryParameter);
    Task<bool> Set(Document document, CancellationToken token);
}
