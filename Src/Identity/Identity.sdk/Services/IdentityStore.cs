using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Services
{
    public class IdentityStore : IIdentityStore
    {
        private readonly IArtifactClient _artifactClient;

        public IdentityStore(IArtifactClient artifactClient)
        {
            _artifactClient = artifactClient;
        }

        public async Task<bool> Delete(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            return await _artifactClient.Delete(id, token: token);
        }

        public async Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            return await _artifactClient.Get(id, token: token);
        }

        public BatchSetCursor<string> List(QueryParameter queryParameter)
        {
            queryParameter.VerifyNotNull(nameof(queryParameter));

            return _artifactClient.List(queryParameter);
        }

        public async Task Set(ArtifactPayload artifactPayload, CancellationToken token = default)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            await _artifactClient.Set(artifactPayload, token);
        }
    }
}
