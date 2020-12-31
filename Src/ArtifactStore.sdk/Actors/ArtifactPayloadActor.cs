using ArtifactStore.sdk.Model;
using ArtifactStore.sdk.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Actors
{
    public class ArtifactPayloadActor : ActorBase, IArtifactPayloadActor
    {
        private readonly IArtifactStorageFactory _artifactStorageProvider;
        private readonly ILogger<ArtifactPayloadActor> _logger;
        private CacheObject<ArtifactPayload> _cache = new CacheObject<ArtifactPayload>(TimeSpan.FromMinutes(10));
        private IArtifactStorage _storage = null!;
        private ArtifactId _artifactId = null!;

        public ArtifactPayloadActor(IArtifactStorageFactory artifactStorageProvider, ILogger<ArtifactPayloadActor> logger)
        {
            artifactStorageProvider.VerifyNotNull(nameof(artifactStorageProvider));
            logger.VerifyNotNull(nameof(logger));
            _artifactStorageProvider = artifactStorageProvider;
            _logger = logger;
        }

        protected override Task OnActivate()
        {
            _artifactId = (ArtifactId)ActorKey.Value;

            _storage = _artifactStorageProvider.Create(_artifactId.Namespace)
                .VerifyNotNull($"Namespace not found for {ActorKey.Value}");

            return Task.CompletedTask;
        }

        public async Task<ArtifactPayload?> Get(CancellationToken token)
        {
            if (_cache.TryGetValue(out ArtifactPayload? value)) return value;

            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
            ArtifactPayload? articlePayload = await _storage.Get(_artifactId, token: token);

            if (articlePayload == null) return null;

            _cache.Set(articlePayload);
            return articlePayload;
        }

        public async Task Set(ArtifactPayload artifactPayload, CancellationToken token)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload))
                .VerifyAssert(x => artifactPayload.Id.ToLower() == ActorKey.Value, $"Id mismatch - id={artifactPayload.Id.ToLower()}, actorKey={ActorKey}");

            _logger.LogTrace($"{nameof(Set)}: Writing {artifactPayload.Id}");
            await _storage.Set(artifactPayload, token);

            _cache.Set(artifactPayload);
        }

        public async Task<bool> Delete(CancellationToken token)
        {
            _cache.Clear();

            _logger.LogTrace($"{nameof(Delete)}: actorKey={ActorKey}");
            bool state = await _storage.Delete(_artifactId, token: token);

            await Deactivate();
            return state;
        }
    }
}