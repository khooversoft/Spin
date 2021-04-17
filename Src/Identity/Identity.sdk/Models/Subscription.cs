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

        public static (IdentityId TenantId, IdentityId SubscriptionId) ParseId(ArtifactId subscription)
        {
            subscription.VerifyNotNull(nameof(subscription));
            subscription.Namespace.VerifyAssert(x => x == "subscription", $"Namespace does not match 'subscription'");
            subscription.PathItems.Count.VerifyAssert(x => x == 2, $"Invalid path for Subscription, {subscription.Path}");

            IdentityId tenantId = (IdentityId)subscription.PathItems[0];
            IdentityId subscriptionId = (IdentityId)subscription.PathItems[1];

            return (tenantId, subscriptionId);
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

        public static bool IsValid(this Subscription subscription) => subscription != null &&
            subscription.TenantId != null &&
            subscription.SubscriptionId != null;

        public static ArtifactId ToArtifactId(this Subscription signature)
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

            return signature.ToArtifactPayload(signature.ToArtifactId());
        }
    }
}
