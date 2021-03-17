using ArtifactStore.sdk.Model;
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
        public string TenantId { get; set; } = null!;

        public string SubscriptionId { get; set; } = null!;

        public string Name { get; set; } = null!;


        public static ArtifactId ToArtifactId(string tenantId, string subscriptionId)
        {
            tenantId.VerifyNotEmpty(nameof(tenantId));
            subscriptionId.VerifyNotEmpty(nameof(subscriptionId));

            return new ArtifactId($"subscription/{tenantId}/{subscriptionId}");
        }
    }

    public static class SubscriptionExtensions
    {
        public static void Verify(this Subscription subscription)
        {
            subscription.VerifyNotNull(nameof(subscription));
            subscription.TenantId.VerifyNotEmpty(nameof(subscription.TenantId));
            subscription.SubscriptionId.VerifyNotEmpty(nameof(subscription.SubscriptionId));
            subscription.Name.VerifyNotEmpty(nameof(subscription.Name));
        }
    }
}
