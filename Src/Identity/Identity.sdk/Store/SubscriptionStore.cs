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
    public class SubscriptionStore
    {
        private readonly IArtifactClient _artifactClient;
        private readonly string _namespace;

        public SubscriptionStore(IArtifactClient artifactClient, string nameSpace)
        {
            artifactClient.VerifyNotNull(nameof(artifactClient));
            nameSpace.VerifyNotNull(nameof(nameSpace));

            _artifactClient = artifactClient;
            _namespace = nameSpace;
        }

        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));

            ArtifactId id = ToArtifact(tenantId, subscriptionId);
            return await _artifactClient.Delete(id, token);
        }

        public async Task<Subscription?> Get(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));

            ArtifactId id = ToArtifact(tenantId, subscriptionId);
            ArtifactPayload? articlePayload = await _artifactClient.Get(id, token);

            if (articlePayload == null) return null;

            return articlePayload.ToSubscription();
        }
        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default)
        {
            queryParameter.VerifyNotNull(nameof(queryParameter));
            BatchSetHttpCursor<string> batch = _artifactClient.List(queryParameter with { Namespace = _namespace });
            return await batch.ToList(token);
        }

        public async Task Set(Subscription subscription, CancellationToken token = default)
        {
            subscription.VerifyNotNull(nameof(subscription));

            ArtifactId id = ToArtifact(subscription.TenantId, subscription.SubscriptionId);
            await _artifactClient.Set(subscription.ToArtifactPayload(id), token);
        }

        private ArtifactId ToArtifact(IdentityId tenantId, IdentityId subscriptionId) => (ArtifactId)$"{_namespace}/{tenantId}/{subscriptionId}";
    }
}