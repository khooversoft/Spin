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
    public class SubscriptionActor : ActorBase, ISubscriptionActor
    {
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<SubscriptionActor> _logger;
        private CacheObject<Subscription> _cache = new CacheObject<Subscription>(TimeSpan.FromMinutes(10));

        public SubscriptionActor(IArtifactClient artifactClient, ILogger<SubscriptionActor> logger)
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
            bool state = await _artifactClient.Delete(new ArtifactId((string)base.ActorKey), token: token);

            return state;
        }

        public async Task<Subscription?> Get(CancellationToken token)
        {
            if (_cache.TryGetValue(out Subscription? value)) return value;

            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
            ArtifactPayload? articlePayload = await _artifactClient.Get(new ArtifactId((string)base.ActorKey), token: token);

            if (articlePayload == null) return null;

            Subscription subscription = articlePayload.ToSubscription();

            _cache.Set(subscription);
            return subscription;
        }

        public async Task Set(Subscription subscription, CancellationToken token)
        {
            subscription.VerifyNotNull(nameof(subscription));
            ((ArtifactId)(string)base.ActorKey == subscription.ToArtifactId()).VerifyAssert(x => x == true, "Subscription does not match actor key");

            _logger.LogTrace($"{nameof(Set)}: actorKey={ActorKey}");

            await _artifactClient.Set(subscription.ToArtifactPayload(subscription.ToArtifactId()), token);
            _cache.Set(subscription);
        }
    }
}