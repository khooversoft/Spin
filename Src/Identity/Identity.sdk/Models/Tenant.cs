using ArtifactStore.sdk.Model;
using Identity.sdk.Types;
using System.Diagnostics.CodeAnalysis;
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
}