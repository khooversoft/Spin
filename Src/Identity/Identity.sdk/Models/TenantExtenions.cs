using ArtifactStore.sdk.Model;
using Identity.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Identity.sdk.Models
{
    public static class TenantExtenions
    {
        public static Tenant Verify(this Tenant tenant)
        {
            tenant.VerifyNotNull(nameof(tenant));
            tenant.TenantId.VerifyNotNull(nameof(tenant.TenantId));
            tenant.Name.VerifyNotEmpty(nameof(tenant.Name));

            return tenant;
        }

        public static bool IsValid(this Tenant tenant) => tenant != null &&
            tenant.TenantId != null &&
            !tenant.Name.IsEmpty();

        public static ArtifactId ToArtifactId(this Tenant tenant)
        {
            tenant.VerifyNotNull(nameof(tenant));
            tenant.Verify();

            return Tenant.ToArtifactId(tenant.TenantId);
        }

        public static Tenant ToTenant(this ArtifactPayload artifactPayload)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            return artifactPayload.DeserializeFromArtifactPayload<Tenant>();
        }

        public static ArtifactPayload ToArtifactPayload(this Tenant tenant)
        {
            tenant.VerifyNotNull(nameof(tenant));

            return tenant.ToArtifactPayload(tenant.ToArtifactId());
        }

        public static IdentityId ParseId(ArtifactId subscription)
        {
            subscription.VerifyNotNull(nameof(subscription));
            subscription.Namespace.VerifyAssert(x => x == "tenant", $"Namespace does not match 'tenant'");
            subscription.PathItems.Count.VerifyAssert(x => x == 1, $"Invalid path for Subscription, {subscription.Path}");

            return (IdentityId)subscription.PathItems[0];
        }
    }
}