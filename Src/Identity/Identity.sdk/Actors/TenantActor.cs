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
    public class TenantActor : ActorBase, ITenantActor
    {
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<TenantActor> _logger;
        private CacheObject<Tenant> _cache = new CacheObject<Tenant>(TimeSpan.FromMinutes(10));

        public TenantActor(IArtifactClient artifactClient, ILogger<TenantActor> logger)
        {
            artifactClient.VerifyNotNull(nameof(artifactClient));
            logger.VerifyNotNull(nameof(logger));

            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task<bool> Delete(CancellationToken token)
        {
            _cache.Clear();

            _logger.LogTrace($"{nameof(Delete)}: actorKey={ActorKey}");
            bool state = await _artifactClient.Delete((ArtifactId)(string)base.ActorKey, token: token);

            return state;
        }

        public async Task<Tenant?> Get(CancellationToken token)
        {
            if (_cache.TryGetValue(out Tenant? value)) return value;

            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
            ArtifactPayload? articlePayload = await _artifactClient.Get((ArtifactId)(string)base.ActorKey, token: token);

            if (articlePayload == null) return null;

            Tenant tenant = articlePayload.ToTenant();

            _cache.Set(tenant);
            return tenant;
        }

        public async Task Set(Tenant tenant, CancellationToken token)
        {
            tenant.VerifyNotNull(nameof(tenant));
            ((ArtifactId)(string)base.ActorKey == tenant.ToArtifactId()).VerifyAssert(x => x == true, "Tenant does not match actor key");

            _logger.LogTrace($"{nameof(Set)}: actorKey={ActorKey}");

            await _artifactClient.Set(tenant.ToArtifactPayload(tenant.ToArtifactId()), token);
            _cache.Set(tenant);
        }
    }
}