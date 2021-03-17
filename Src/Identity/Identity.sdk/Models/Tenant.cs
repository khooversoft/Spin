using ArtifactStore.sdk.Model;
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
        public string TenantId { get; set; } = null!;

        public string Name { get; set; } = null!;


        public static ArtifactId ToArtifactId(string tenantId)
        {
            tenantId.VerifyNotEmpty(nameof(tenantId));

            return new ArtifactId($"tenant/{tenantId}");
        }
    }

    public static class TenantExtenions
    {
        public static void Verify(this Tenant tenant)
        {
            tenant.VerifyNotNull(nameof(tenant));
            tenant.TenantId.VerifyNotEmpty(nameof(tenant.TenantId));
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
