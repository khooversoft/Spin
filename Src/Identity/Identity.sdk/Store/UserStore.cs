using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Types;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Store
{
    public class UserStore
    {
        private readonly IArtifactClient _artifactClient;
        private readonly string _namespace;

        public UserStore(IArtifactClient artifactClient, string nameSpace)
        {
            artifactClient.VerifyNotNull(nameof(artifactClient));
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            _artifactClient = artifactClient;
            _namespace = nameSpace;
        }

        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));
            userId.VerifyNotNull(nameof(userId));

            ArtifactId id = ToArtifact(tenantId, subscriptionId, userId);
            return await _artifactClient.Delete(id, token);
        }

        public async Task<User?> Get(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));
            userId.VerifyNotNull(nameof(userId));

            ArtifactId id = ToArtifact(tenantId, subscriptionId, userId);
            ArtifactPayload? articlePayload = await _artifactClient.Get(id, token);

            if (articlePayload == null) return null;

            return articlePayload.ToUser();
        }

        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default)
        {
            queryParameter.VerifyNotNull(nameof(queryParameter));
            BatchSetCursor<string> batch = _artifactClient.List(queryParameter with { Namespace = _namespace });
            return await batch.ToList(token);
        }

        public async Task Set(User user, CancellationToken token = default)
        {
            user.VerifyNotNull(nameof(user));

            ArtifactId id = ToArtifact(user.TenantId, user.SubscriptionId, user.UserId);
            await _artifactClient.Set(user.ToArtifactPayload(id), token);
        }

        private ArtifactId ToArtifact(IdentityId tenantId, IdentityId subscriptionId, UserId userId) => (ArtifactId)$"{_namespace}/{tenantId}/{subscriptionId}/{userId}";
    }
}