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
    public class UserActor : ActorBase, IUserActor
    {
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<UserActor> _logger;
        private CacheObject<User> _cache = new CacheObject<User>(TimeSpan.FromMinutes(10));

        public UserActor(IArtifactClient artifactClient, ILogger<UserActor> logger)
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

        public async Task<User?> Get(CancellationToken token)
        {
            if (_cache.TryGetValue(out User? value)) return value;

            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
            ArtifactPayload? articlePayload = await _artifactClient.Get(new ArtifactId((string)base.ActorKey), token: token);

            if (articlePayload == null) return null;

            User user = articlePayload.ToUser();

            _cache.Set(user);
            return user;
        }

        public async Task Set(User user, CancellationToken token)
        {
            user.VerifyNotNull(nameof(user));

            _logger.LogTrace($"{nameof(Set)}: actorKey={ActorKey}");

            await _artifactClient.Set(user.ToArtifactPayload(user.ToArtifactId()), token);
            _cache.Set(user);
        }
    }
}