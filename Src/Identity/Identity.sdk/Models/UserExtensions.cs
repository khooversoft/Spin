using ArtifactStore.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
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

            return artifactPayload.DeserializePayload<User>();
        }

        public static ArtifactPayload ToArtifactPayload(this User user)
        {
            user.VerifyNotNull(nameof(user));

            return new ArtifactPayloadBuilder()
                .SetId(user.ToArtifactId())
                .SetPayload(user)
                .Build();
        }
    }
}
