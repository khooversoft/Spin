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
    public class SignatureStore
    {
        private readonly IArtifactClient _artifactClient;
        private readonly string _namespace;

        public SignatureStore(IArtifactClient artifactClient, string nameSpace)
        {
            artifactClient.VerifyNotNull(nameof(artifactClient));
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            _namespace = nameSpace;
            _artifactClient = artifactClient;
        }

        public async Task<bool> Delete(IdentityId signatureId, CancellationToken token = default)
        {
            signatureId.VerifyNotNull(nameof(signatureId));

            ArtifactId id = ToArtifact(signatureId);
            return await _artifactClient.Delete(id, token);
        }

        public async Task<Signature?> Get(IdentityId tenantId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));

            ArtifactId id = ToArtifact(tenantId);
            ArtifactPayload? articlePayload = await _artifactClient.Get(id, token);

            if (articlePayload == null) return null;

            return articlePayload.ToSignature();
        }

        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default)
        {
            queryParameter.VerifyNotNull(nameof(queryParameter));
            BatchSetHttpCursor<string> batch = _artifactClient.List(queryParameter with { Namespace = _namespace });
            return await batch.ToList(token);
        }

        public async Task Set(Signature signature, CancellationToken token = default)
        {
            signature.VerifyNotNull(nameof(signature));

            ArtifactId id = ToArtifact(signature.SignatureId);
            await _artifactClient.Set(signature.ToArtifactPayload(id), token);
        }

        private ArtifactId ToArtifact(IdentityId identityId) => (ArtifactId)$"{_namespace}/{identityId}";
    }
}