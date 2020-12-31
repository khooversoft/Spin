using ArtifactStore.sdk.Model;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;

namespace ArtifactStore.sdk.Actors
{
    public interface IArtifactPayloadActor : IActor
    {
        Task<bool> Delete(CancellationToken token);

        Task<ArtifactPayload?> Get(CancellationToken token);

        Task Set(ArtifactPayload articlePayload, CancellationToken token);
    }
}