using ArtifactStore.sdk.Model;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Services
{
    public interface IIdentityStore
    {
        Task<bool> Delete(ArtifactId id, CancellationToken token = default);
        Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default);
        BatchSetCursor<string> List(QueryParameter queryParameter);
        Task Set(ArtifactPayload artifactPayload, CancellationToken token = default);
    }
}