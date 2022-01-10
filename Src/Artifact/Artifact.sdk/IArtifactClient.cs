using System.Threading;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Tools;

namespace Artifact.sdk
{
    public interface IArtifactClient
    {
        Task<bool> Delete(DocumentId id, CancellationToken token = default);
        Task<Document?> Get(DocumentId id, CancellationToken token = default);
        BatchSetCursor<string> Search(QueryParameter queryParameter);
        Task Set(Document document, CancellationToken token = default);
    }
}