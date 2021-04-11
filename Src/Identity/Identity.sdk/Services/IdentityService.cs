//using ArtifactStore.sdk.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Actor.Host;

//namespace Identity.sdk.Services
//{
//    public class IdentityService
//    {
//        public IActorHost? _actorHost;
//        private readonly IArtifactStorageFactory _artifactStorageProvider;
//        private readonly ILogger<ArtifactStoreService> _logger;
//        private readonly ConcurrentDictionary<string, IArtifactStorage> _namespaceCache = new ConcurrentDictionary<string, IArtifactStorage>(StringComparer.OrdinalIgnoreCase);

//        public IdentityService(IActorHost actorHost, IArtifactStorageFactory artifactStorageProvider, ILogger<ArtifactStoreService> logger)
//        {
//            _actorHost = actorHost;
//            _artifactStorageProvider = artifactStorageProvider;
//            _logger = logger;
//        }

//        public async Task<bool> Delete(ArtifactId id, CancellationToken token = default)
//        {
//            id.VerifyNotNull(nameof(id));

//            var actorKey = new ActorKey((string)id);
//            _logger.LogTrace($"{nameof(Delete)}: actorKey={actorKey}, id={id.Id}");

//            IArtifactPayloadActor actor = _actorHost!.GetActor<IArtifactPayloadActor>(actorKey);
//            return await actor.Delete(token);
//        }

//        public async Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default)
//        {
//            id.VerifyNotNull(nameof(id));

//            var actorKey = new ActorKey((string)id);
//            _logger.LogTrace($"{nameof(Get)}: actorKey={actorKey}, id={id.Id}");

//            IArtifactPayloadActor actor = _actorHost!.GetActor<IArtifactPayloadActor>(actorKey);
//            return await actor.Get(token);
//        }

//        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default)
//        {
//            queryParameter
//                .VerifyNotNull(nameof(queryParameter))
//                .VerifyAssert(x => !x.Namespace.IsEmpty(), x => $"{nameof(x.Namespace)} is required");

//            IArtifactStorage getStorage(string nameSpace) => _artifactStorageProvider.Create(queryParameter.Namespace!)
//                .VerifyNotNull($"{queryParameter.Namespace} {nameof(queryParameter.Namespace)} not found");

//            IArtifactStorage artifactStorage = _namespaceCache.GetOrAdd(queryParameter.Namespace!, x => getStorage(queryParameter.Namespace!));

//            return await artifactStorage.List(queryParameter, token);
//        }

//        public async Task Set(ArtifactPayload record, CancellationToken token = default)
//        {
//            record.VerifyNotNull(nameof(record));

//            var actorKey = new ActorKey(new ArtifactId(record.Id).ToString());
//            _logger.LogTrace($"{nameof(Set)}: actorKey={actorKey}, id={record.Id}");

//            IArtifactPayloadActor actor = _actorHost!.GetActor<IArtifactPayloadActor>(actorKey);
//            await actor.Set(record, token);
//        }
//    }
//}
