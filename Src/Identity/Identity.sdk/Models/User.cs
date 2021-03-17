using ArtifactStore.sdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
    public record User
    {
        public string TenantId { get; set; } = null!;

        public string SubscriptionId { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string DomainId { get; set; } = null!;

        public string Name { get; set; } = null!;

        public IReadOnlyList<Signature>? PublicKeySignatures { get; set; }


        public static ArtifactId ToArtifactId(string tenantId, string subscriptionId, string userId)
        {
            tenantId.VerifyNotEmpty(nameof(tenantId));
            subscriptionId.VerifyNotEmpty(nameof(subscriptionId));
            userId.VerifyNotEmpty(nameof(userId));

            return new ArtifactId($"subscription/{tenantId}/{subscriptionId}/{userId}");
        }
    }

    public static class UserExtensions
    {
        public static void Verify(this User user)
        {
            user.VerifyNotNull(nameof(user));
            user.TenantId.VerifyNotEmpty(nameof(user.TenantId));
            user.SubscriptionId.VerifyNotEmpty(nameof(user.SubscriptionId));
            user.UserId.VerifyNotEmpty(nameof(user.UserId));
            user.DomainId.VerifyNotEmpty(nameof(user.DomainId));
            user.Name.VerifyNotEmpty(nameof(user.Name));
        }
    }
}
