using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Types;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Store
{
    public class TenantStore
    {
        private readonly IArtifactClient _artifactClient;
        private readonly string _namespace;

        public TenantStore(IArtifactClient artifactClient, string nameSpace)
        {
            artifactClient.VerifyNotNull(nameof(artifactClient));
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            _artifactClient = artifactClient;
            _namespace = nameSpace;
        }

        public async Task<bool> Delete(IdentityId tenantId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));

            ArtifactId id = ToArtifact(tenantId);
            return await _artifactClient.Delete(id, token);
        }

        public async Task<Tenant?> Get(IdentityId tenantId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));

            ArtifactId id = ToArtifact(tenantId);
            ArtifactPayload? articlePayload = await _artifactClient.Get(id, token);

            if (articlePayload == null) return null;

            return articlePayload.ToTenant();
        }

        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default)
        {
            queryParameter.VerifyNotNull(nameof(queryParameter));
            BatchSetHttpCursor<string> batch = _artifactClient.List(queryParameter with { Namespace = _namespace });
            return await batch.ToList(token);
        }

        public async Task Set(Tenant tenant, CancellationToken token = default)
        {
            tenant.VerifyNotNull(nameof(tenant));

            ArtifactId id = ToArtifact(tenant.TenantId);
            await _artifactClient.Set(tenant.ToArtifactPayload(id), token);
        }

        private ArtifactId ToArtifact(IdentityId identityId) => (ArtifactId)$"{_namespace}/{identityId}";
    }
}