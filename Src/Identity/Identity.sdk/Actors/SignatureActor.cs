using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Tools;

namespace Identity.sdk.Actors
{
    public class SignatureActor : ActorBase, ISignatureActor
    {
        private readonly IArtifactClient _artificatClient;
        private readonly ILogger<SignatureActor> _logger;
        private CacheObject<Signature> _cache = new CacheObject<Signature>(TimeSpan.FromMinutes(10));

        public SignatureActor(IArtifactClient artifactClient, ILogger<SignatureActor> logger)
        {
            artifactClient.VerifyNotNull(nameof(artifactClient));
            logger.VerifyNotNull(nameof(logger));

            _artificatClient = artifactClient;
            _logger = logger;
        }

        public async Task<bool> Delete(CancellationToken token)
        {
            _cache.Clear();

            _logger.LogTrace($"{nameof(Delete)}: actorKey={ActorKey}");
            bool state = await _artificatClient.Delete(new ArtifactId((string)base.ActorKey), token: token);

            return state;
        }

        public async Task<Signature?> Get(CancellationToken token)
        {
            if (_cache.TryGetValue(out Signature? value)) return value;

            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
            ArtifactPayload? articlePayload = await _artificatClient.Get(new ArtifactId((string)base.ActorKey), token: token);

            if (articlePayload == null) return null;

            Signature signature = articlePayload.ToSignature();

            _cache.Set(signature);
            return signature;
        }

        public async Task Set(Signature signature, CancellationToken token)
        {
            signature.VerifyNotNull(nameof(signature));

            _logger.LogTrace($"{nameof(Set)}: actorKey={ActorKey}");

            await _artificatClient.Set(signature.ToArtifactPayload(signature.ToArtifactId()), token);
            _cache.Set(signature);
        }
    }
}