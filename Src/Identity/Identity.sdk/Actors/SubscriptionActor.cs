//using ArtifactStore.sdk.Model;
//using Identity.sdk.Models;
//using Identity.sdk.Services;
//using Identity.sdk.Types;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Toolbox.Actor;
//using Toolbox.Tools;

//namespace Identity.sdk.Actors
//{
//    public class SubscriptionActor : ActorBase, ISubscriptionActor
//    {
//        private readonly IIdentityStore _identityStore;
//        private readonly ILogger<SubscriptionActor> _logger;
//        private CacheObject<Subscription> _cache = new CacheObject<Subscription>(TimeSpan.FromMinutes(10));

//        public SubscriptionActor(IIdentityStore identityStore, ILogger<SubscriptionActor> logger)
//        {
//            identityStore.VerifyNotNull(nameof(identityStore));
//            logger.VerifyNotNull(nameof(logger));

//            _identityStore = identityStore;
//            _logger = logger;
//        }

//        public async Task<bool> Delete(CancellationToken token)
//        {
//            _cache.Clear();

//            _logger.LogTrace($"{nameof(Delete)}: actorKey={ActorKey}");
//            bool state = await _identityStore.Delete(Subscription.ToArtifactId((IdentityId)(string)base.ActorKey), token: token);

//            return state;
//        }

//        public async Task<Subscription?> Get(CancellationToken token)
//        {
//            if (_cache.TryGetValue(out Subscription? value)) return value;

//            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
//            ArtifactPayload? articlePayload = await _identityStore.Get(Signature.ToArtifactId((IdentityId)(string)base.ActorKey), token: token);

//            if (articlePayload == null) return null;

//            Subscription subscription = articlePayload.ToSubscription();

//            _cache.Set(subscription);
//            return subscription;
//        }

//        public async Task Set(Subscription subscription, CancellationToken token)
//        {
//            subscription.VerifyNotNull(nameof(subscription));

//            _logger.LogTrace($"{nameof(Set)}: actorKey={ActorKey}");

//            await _identityStore.Set(subscription.ToArtifactPayload(subscription.GetArtifactId()), token);
//            _cache.Set(subscription);
//        }
//    }
//}
