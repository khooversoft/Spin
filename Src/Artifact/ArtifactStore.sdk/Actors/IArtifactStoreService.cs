using ArtifactStore.sdk.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;

namespace ArtifactStore.sdk.Actors
{
    public interface IArtifactStoreService
    {
        Task<bool> Delete(ArtifactId id, CancellationToken token = default);

        Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default);

        Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default);

        Task Set(ArtifactPayload record, CancellationToken token = default);
    }
}