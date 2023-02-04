using Toolbox.Models;
using Toolbox.Protocol;

namespace Toolbox.Store;

public interface IDocumentStore
{
    Task<bool> Delete(DocumentId id, CancellationToken token = default);
    Task<Document?> Get(DocumentId id, CancellationToken token = default);
    Task<IReadOnlyList<StorePathItem>> Search(QueryParameter queryParameter);
    Task Set(Document document, CancellationToken token = default);
}
