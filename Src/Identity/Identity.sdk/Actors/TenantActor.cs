using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Services;
using Identity.sdk.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Tools;

namespace Identity.sdk.Actors
{
    public class TenantActor : ActorBase, ITenantActor
    {
        private readonly IIdentityStore _identityStore;
        private readonly ILogger<TenantActor> _logger;
        private CacheObject<Tenant> _cache = new CacheObject<Tenant>(TimeSpan.FromMinutes(10));

        public TenantActor(IIdentityStore identityStore, ILogger<TenantActor> logger)
        {
            identityStore.VerifyNotNull(nameof(identityStore));
            logger.VerifyNotNull(nameof(logger));

            _identityStore = identityStore;
            _logger = logger;
        }

        public async Task<bool> Delete(CancellationToken token)
        {
            _cache.Clear();

            _logger.LogTrace($"{nameof(Delete)}: actorKey={ActorKey}");
            bool state = await _identityStore.Delete(Tenant.ToArtifactId((IdentityId)(string)base.ActorKey), token: token);

            return state;
        }

        public async Task<Tenant?> Get(CancellationToken token)
        {
            if (_cache.TryGetValue(out Tenant? value)) return value;

            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
            ArtifactPayload? articlePayload = await _identityStore.Get(Tenant.ToArtifactId((IdentityId)(string)base.ActorKey), token: token);

            if (articlePayload == null) return null;

            Tenant tenant = articlePayload.ToTenant();

            _cache.Set(tenant);
            return tenant;
        }

        public async Task Set(Tenant tenant, CancellationToken token)
        {
            tenant.VerifyNotNull(nameof(tenant));

            _logger.LogTrace($"{nameof(Set)}: actorKey={ActorKey}");

            await _identityStore.Set(tenant.ToArtifactPayload(tenant.GetArtifactId()), token);
            _cache.Set(tenant);
        }
    }
}
