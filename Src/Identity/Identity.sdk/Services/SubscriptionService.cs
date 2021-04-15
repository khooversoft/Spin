using ArtifactStore.sdk.Client;
using ArtifactStore.sdk.Model;
using Identity.sdk.Actors;
using Identity.sdk.Client;
using Identity.sdk.Models;
using Identity.sdk.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Actor.Host;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Services
{
    public class SubscriptionService
    {
        private readonly IActorHost _actorHost;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(IActorHost actorHost, IArtifactClient artifactClient, ILogger<SubscriptionService> logger)
        {
            _actorHost = actorHost;
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));

            var actorKey = new ActorKey((string)Subscription.ToArtifactId(tenantId, subscriptionId));
            _logger.LogTrace($"{nameof(Delete)}: actorKey={actorKey}, TenantId={tenantId}, Subscription={subscriptionId}");

            ISubscriptionActor actor = _actorHost.GetActor<ISubscriptionActor>(actorKey);
            return await actor.Delete(token);
        }

        public async Task<Subscription?> Get(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));

            var actorKey = new ActorKey((string)Subscription.ToArtifactId(tenantId, subscriptionId));
            _logger.LogTrace($"{nameof(Get)}: actorKey={actorKey}, TenantId={tenantId}, Subscription={subscriptionId}");

            ISubscriptionActor actor = _actorHost.GetActor<ISubscriptionActor>(actorKey);
            return await actor.Get(token);
        }

        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default)
        {
            queryParameter
                .VerifyNotNull(nameof(queryParameter))
                .VerifyAssert(x => !x.Namespace.IsEmpty(), x => $"{nameof(x.Namespace)} is required");

            var list = new List<string>();
            BatchSetCursor<string> batch = _artifactClient.List(queryParameter);

            while (true)
            {
                BatchSet<string>? result = await batch.ReadNext(token);
                if (result.IsEndSignaled) break;

                list.AddRange(result.Records);
            }

            return list;
        }

        public async Task Set(Subscription subscription, CancellationToken token = default)
        {
            subscription.VerifyNotNull(nameof(subscription));

            var actorKey = new ActorKey(subscription.ToArtifactId().ToString());
            _logger.LogTrace($"{nameof(Set)}: actorKey={actorKey}, id={subscription.TenantId}");

            ISubscriptionActor actor = _actorHost.GetActor<ISubscriptionActor>(actorKey);
            await actor.Set(subscription, token);
        }
    }
}
