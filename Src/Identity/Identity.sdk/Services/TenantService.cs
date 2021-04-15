using ArtifactStore.sdk.Client;
using Identity.sdk.Actors;
using Identity.sdk.Models;
using Identity.sdk.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;
using Toolbox.Actor.Host;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Services
{
    public class TenantService
    {
        private readonly IActorHost _actorHost;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<TenantService> _logger;

        public TenantService(IActorHost actorHost, IArtifactClient artifactClient, ILogger<TenantService> logger)
        {
            _actorHost = actorHost;
            _artifactClient = artifactClient;
            _logger = logger;
        }

        public async Task<bool> Delete(IdentityId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            var actorKey = new ActorKey((string)id);
            _logger.LogTrace($"{nameof(Delete)}: actorKey={actorKey}, id={id}");

            ITenantActor actor = _actorHost.GetActor<ITenantActor>(actorKey);
            return await actor.Delete(token);
        }

        public async Task<Tenant?> Get(IdentityId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            var actorKey = new ActorKey((string)id);
            _logger.LogTrace($"{nameof(Get)}: actorKey={actorKey}, id={id}");

            ITenantActor actor = _actorHost.GetActor<ITenantActor>(actorKey);
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

        public async Task Set(Tenant tenant, CancellationToken token = default)
        {
            tenant.VerifyNotNull(nameof(tenant));

            var actorKey = new ActorKey(tenant.ToArtifactId().ToString());
            _logger.LogTrace($"{nameof(Set)}: actorKey={actorKey}, id={tenant.TenantId}");

            ITenantActor actor = _actorHost.GetActor<ITenantActor>(actorKey);
            await actor.Set(tenant, token);
        }
    }
}