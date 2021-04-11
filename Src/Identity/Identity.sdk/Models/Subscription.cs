using ArtifactStore.sdk.Model;
using Identity.sdk.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
    public record Subscription
    {
        public IdentityId TenantId { get; set; } = null!;

        public IdentityId SubscriptionId { get; set; } = null!;

        public string Name { get; set; } = null!;


        public static ArtifactId ToArtifactId(IdentityId tenantId, IdentityId subscriptionId)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));

            return new ArtifactId($"subscription/{tenantId}/{subscriptionId}");
        }
    }

    public static class SubscriptionExtensions
    {
        public static void Verify(this Subscription subscription)
        {
            subscription.VerifyNotNull(nameof(subscription));
            subscription.TenantId.VerifyNotNull(nameof(subscription.TenantId));
            subscription.SubscriptionId.VerifyNotNull(nameof(subscription.SubscriptionId));
            subscription.Name.VerifyNotEmpty(nameof(subscription.Name));
        }

        public static ArtifactId GetArtifactId(this Subscription signature)
        {
            signature.VerifyNotNull(nameof(signature));
            signature.Verify();

            return Subscription.ToArtifactId(signature.TenantId, signature.SubscriptionId);
        }

        public static Subscription ToSubscription(this ArtifactPayload artifactPayload)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            return artifactPayload.DeserializeFromArtifactPayload<Subscription>();
        }

        public static ArtifactPayload ToArtifactPayload(this Subscription signature)
        {
            signature.VerifyNotNull(nameof(signature));

            return signature.ToArtifactPayload(signature.GetArtifactId());
        }
    }
}
