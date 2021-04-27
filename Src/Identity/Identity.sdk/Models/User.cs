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
    public record User
    {
        public IdentityId TenantId { get; set; } = null!;

        public IdentityId SubscriptionId { get; set; } = null!;

        public UserId UserId { get; set; } = null!;

        public string Name { get; set; } = null!;

        public IReadOnlyList<Signature>? PublicKeySignatures { get; set; }


        public static ArtifactId ToArtifactId(IdentityId tenantId, IdentityId subscriptionId, UserId userId)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));
            userId.VerifyNotNull(nameof(userId));

            return new ArtifactId($"user/{tenantId}/{subscriptionId}/{userId}");
        }
    }
}
