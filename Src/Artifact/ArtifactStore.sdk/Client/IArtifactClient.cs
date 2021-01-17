using ArtifactStore.sdk.Model;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Tools;

namespace nBlog.sdk.Client
{
    public interface IArtifactClient
    {
        Task<bool> Delete(ArtifactId id, CancellationToken token = default);
        Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default);
        BatchSetCursor<string> List(QueryParameter queryParameter);
        Task Set(ArtifactPayload articlePayload, CancellationToken token = default);
    }
}