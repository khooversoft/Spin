using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Services
{
    public class ArtifactStorageFactory : DataLakeStoreFactory, IArtifactStorageFactory
    {
        public ArtifactStorageFactory(DataLakeNamespaceOption dataLakeNamespaceOption, ILoggerFactory loggerFactory)
            : base(dataLakeNamespaceOption, loggerFactory)
        {
        }

        public new IArtifactStorage? Create(string nameSpace)
        {
            nameSpace.VerifyNotEmpty(nameof(nameSpace));

            if (!base.TryGetValue(nameSpace, out DataLakeNamespace? subject)) return null;

            var storage = new DataLakeStore(subject.Store, _loggerFactory.CreateLogger<DataLakeStore>());
            return new ArtifactStorage(storage, subject.PathRoot, _loggerFactory.CreateLogger<ArtifactStorage>());
        }
    }
}