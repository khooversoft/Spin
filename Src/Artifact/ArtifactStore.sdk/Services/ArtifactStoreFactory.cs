using System;
using System.Collections.Concurrent;
using ArtifactStore.sdk.Model;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Services
{
    public class ArtifactStoreFactory : IArtifactStoreFactory
    {
        private readonly IDataLakeStoreFactory _dataLakeStoreFactory;
        private readonly ConcurrentDictionary<string, IArtifactStore> _cache = new ConcurrentDictionary<string, IArtifactStore>(StringComparer.OrdinalIgnoreCase);

        public ArtifactStoreFactory(IDataLakeStoreFactory dataLakeStoreFactory)
        {
            dataLakeStoreFactory.VerifyNotNull(nameof(dataLakeStoreFactory));

            _dataLakeStoreFactory = dataLakeStoreFactory;
        }

        public IArtifactStore Create(string nameSpace)
        {
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            IArtifactStore? store;

            if (_cache.TryGetValue(nameSpace, out store)) return store;

            IDataLakeStore storage = _dataLakeStoreFactory.CreateStore(nameSpace)
                .VerifyNotNull($"Cannot create store for Namespace {nameSpace}");

            store = new ArtifactStore(storage);
            _cache[nameSpace] = store;

            return store;
        }

        public IArtifactStore Create(ArtifactId artifactId) => Create(artifactId.Namespace);
    }
}