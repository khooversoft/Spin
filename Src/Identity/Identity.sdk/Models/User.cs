using ArtifactStore.sdk.Model;
using Identity.sdk.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
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

    public static class UserExtensions
    {
        public static void Verify(this User user)
        {
            user.VerifyNotNull(nameof(user));
            user.TenantId.VerifyNotNull(nameof(user.TenantId));
            user.SubscriptionId.VerifyNotNull(nameof(user.SubscriptionId));
            user.UserId.VerifyNotNull(nameof(user.UserId));
            user.Name.VerifyNotEmpty(nameof(user.Name));
        }

        public static bool IsValid(this User user) =>
            user != null &&
            user.TenantId != null &&
            user.SubscriptionId != null &&
            user.UserId != null &&
            user.PublicKeySignatures != null &&
            user.PublicKeySignatures.Count > 0 &&
            !user.Name.IsEmpty();

        public static ArtifactId ToArtifactId(this User user)
        {
            user.VerifyNotNull(nameof(user));
            user.Verify();

            return User.ToArtifactId(user.TenantId, user.SubscriptionId, user.UserId);
        }

        public static User ToUser(this ArtifactPayload artifactPayload)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            return artifactPayload.DeserializeFromArtifactPayload<User>();
        }

        public static ArtifactPayload ToArtifactPayload(this User user)
        {
            user.VerifyNotNull(nameof(user));

            return user.ToArtifactPayload(user.ToArtifactId());
        }
    }
}
