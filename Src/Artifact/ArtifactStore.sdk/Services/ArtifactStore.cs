using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArtifactStore.sdk.Model;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Services
{
    public class ArtifactStore : IArtifactStore
    {
        private readonly IDataLakeStore _dataLakeStore;
        private const string _extension = ".json";

        public ArtifactStore(IDataLakeStore dataLakeStore)
        {
            _dataLakeStore = dataLakeStore;
        }

        public Task<bool> Delete(ArtifactId id, CancellationToken token = default) => _dataLakeStore.Delete(PathTools.SetExtension(id.Path, _extension), token);

        public async Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            byte[] fileData = await _dataLakeStore.Read(PathTools.SetExtension(id.Path, _extension), token);
            if (fileData == null || fileData.Length == 0) return null;

            return new ArtifactPayloadBuilder()
                .SetId(id)
                .SetPayload(fileData)
                .Build();

            //return fileData.ToArtifactPayload(id);
        }

        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default) =>
            (await _dataLakeStore.Search(queryParameter, x => x.IsDirectory == false, true, token))
                .Select(x => x.Name.EndsWith(_extension) ? x.Name[0..^_extension.Length] : x.Name)
                .ToList();

        public async Task Set(ArtifactPayload artifactPayload, CancellationToken token = default)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            ArtifactId artifactId = new ArtifactId(artifactPayload.Id);
            await _dataLakeStore.Write(PathTools.SetExtension(artifactId.Path, _extension), artifactPayload.PayloadToBytes(), true, token);
        }
    }
}