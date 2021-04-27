using ArtifactStore.sdk.Model;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
    public static class SubscriptionExtensions
    {
        public static void Verify(this Subscription subscription)
        {
            subscription.VerifyNotNull(nameof(subscription));
            subscription.TenantId.VerifyNotNull(nameof(subscription.TenantId));
            subscription.SubscriptionId.VerifyNotNull(nameof(subscription.SubscriptionId));
            subscription.Name.VerifyNotEmpty(nameof(subscription.Name));
        }

        public static bool IsValid(this Subscription subscription) =>
            subscription != null &&
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
