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
    public record Tenant
    {
        public IdentityId TenantId { get; set; } = null!;

        public string Name { get; set; } = null!;


        public static ArtifactId ToArtifactId(IdentityId tenantId)
        {
            tenantId.VerifyNotNull(nameof(tenantId));

            return new ArtifactId($"tenant/{tenantId}");
        }
    }

    public static class TenantExtenions
    {
        public static void Verify(this Tenant tenant)
        {
            tenant.VerifyNotNull(nameof(tenant));
            tenant.TenantId.VerifyNotNull(nameof(tenant.TenantId));
            tenant.Name.VerifyNotEmpty(nameof(tenant.Name));
        }

        public static ArtifactId GetArtifactId(this Tenant tenant)
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

            return tenant.ToArtifactPayload(tenant.GetArtifactId());
        }
    }
}
