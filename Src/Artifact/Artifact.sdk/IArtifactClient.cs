using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Toolbox.Tools;
using Toolbox.Abstractions;

namespace Artifact.sdk;

public interface IArtifactClient
{
    Task<bool> Delete(DocumentId id, CancellationToken token = default);
    Task<Document?> Get(DocumentId id, CancellationToken token = default);
    BatchSetCursor<DatalakePathItem> Search(QueryParameter queryParameter);
    Task Set(Document document, CancellationToken token = default);
}
