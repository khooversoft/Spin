using ArtifactStore.sdk.Client;
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
    public class UserService
    {
        private readonly IActorHost _actorHost;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<UserService> _logger;

        public UserService(IActorHost actorHost, IArtifactClient artifactClient, ILogger<UserService> logger)
        {
            _actorHost = actorHost;
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));
            userId.VerifyNotNull(nameof(userId));

            var actorKey = new ActorKey((string)User.ToArtifactId(tenantId, subscriptionId, userId));
            _logger.LogTrace($"{nameof(Delete)}: actorKey={actorKey}, TenantId={tenantId}, Subscription={subscriptionId}, UserId={userId}");

            IUserActor actor = _actorHost.GetActor<IUserActor>(actorKey);
            return await actor.Delete(token);
        }

        public async Task<User?> Get(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default)
        {
            tenantId.VerifyNotNull(nameof(tenantId));
            subscriptionId.VerifyNotNull(nameof(subscriptionId));
            userId.VerifyNotNull(nameof(userId));

            var actorKey = new ActorKey((string)User.ToArtifactId(tenantId, subscriptionId, userId));
            _logger.LogTrace($"{nameof(Delete)}: actorKey={actorKey}, TenantId={tenantId}, Subscription={subscriptionId}, UserId={userId}");

            IUserActor actor = _actorHost.GetActor<IUserActor>(actorKey);
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

        public async Task Set(User user, CancellationToken token = default)
        {
            user.VerifyNotNull(nameof(user));

            var actorKey = new ActorKey(user.ToArtifactId().ToString());
            _logger.LogTrace($"{nameof(Set)}: actorKey={actorKey}, id={user.TenantId}");

            IUserActor actor = _actorHost.GetActor<IUserActor>(actorKey);
            await actor.Set(user, token);
        }

    }
}
