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
    public class SignatureActor : ActorBase, ISignatureActor
    {
        private readonly IIdentityStore _identityStore;
        private readonly ILogger<SignatureActor> _logger;
        private CacheObject<Signature> _cache = new CacheObject<Signature>(TimeSpan.FromMinutes(10));

        public SignatureActor(IIdentityStore identityStore, ILogger<SignatureActor> logger)
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

        public async Task<Signature?> Get(CancellationToken token)
        {
            if (_cache.TryGetValue(out Signature? value)) return value;

            _logger.LogTrace($"{nameof(Get)}: actorKey={ActorKey}");
            ArtifactPayload? articlePayload = await _identityStore.Get(Signature.ToArtifactId((IdentityId)(string)base.ActorKey), token: token);

            if (articlePayload == null) return null;

            Signature signature = articlePayload.ToSignature();

            _cache.Set(signature);
            return signature;
        }

        public async Task Set(Signature signature, CancellationToken token)
        {
            signature.VerifyNotNull(nameof(signature));

            _logger.LogTrace($"{nameof(Set)}: actorKey={ActorKey}");

            await _identityStore.Set(signature.ToArtifactPayload(signature.GetArtifactId()), token);
            _cache.Set(signature);
        }
    }
}
