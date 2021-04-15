using ArtifactStore.sdk.Client;
using Identity.sdk.Actors;
using Identity.sdk.Client;
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
    public class SignatureService
    {
        private readonly IActorHost _actorHost;
        private readonly IArtifactClient _artifactClient;
        private readonly ILogger<SignatureService> _logger;

        public SignatureService(IActorHost actorHost, IArtifactClient artifactClient, ILogger<SignatureService> logger)
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

            ISignatureActor actor = _actorHost.GetActor<ISignatureActor>(actorKey);
            return await actor.Delete(token);
        }

        public async Task<Signature?> Get(IdentityId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            var actorKey = new ActorKey((string)id);
            _logger.LogTrace($"{nameof(Get)}: actorKey={actorKey}, id={id}");

            ISignatureActor actor = _actorHost.GetActor<ISignatureActor>(actorKey);
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

        public async Task Set(Signature signature, CancellationToken token = default)
        {
            signature.VerifyNotNull(nameof(signature));

            var actorKey = new ActorKey(signature.ToArtifactId().ToString());
            _logger.LogTrace($"{nameof(Set)}: actorKey={actorKey}, id={signature.SignatureId}");

            ISignatureActor actor = _actorHost.GetActor<ISignatureActor>(actorKey);
            await actor.Set(signature, token);
        }
    }
}