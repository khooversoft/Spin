using ArtifactStore.sdk.Extensions;
using ArtifactStore.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Services
{
    public class ArtifactStorage : IArtifactStorage
    {
        private readonly IDataLakeStore _dataLakeStore;
        private readonly string _pathRoot;
        private readonly ILogger<ArtifactStorage> _logger;

        public ArtifactStorage(IDataLakeStore dataLakeStore, string? pathRoot, ILogger<ArtifactStorage> logger)
        {
            _dataLakeStore = dataLakeStore;
            _pathRoot = pathRoot.ToNullIfEmpty() ?? string.Empty;
            _logger = logger;
        }

        public async Task<bool> Delete(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            return await _dataLakeStore.Delete(RealPath(id), token: token);
        }

        public async Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));

            byte[] fileData = await _dataLakeStore.Read(RealPath(id), token: token);
            if (fileData == null || fileData.Length == 0) return null;

            return fileData.ToArtifactPayload(id);
        }

        public async Task<IReadOnlyList<string>> List(QueryParameter queryParameter, CancellationToken token = default) =>
            (await _dataLakeStore.Search(RealPath(queryParameter.Filter), x => x.IsDirectory == false, true, token))
                .Select(x => RemovePathRoot(x.Name))
                .Skip(queryParameter.Index)
                .Take(queryParameter.Count)
                .ToList();

        public async Task Set(ArtifactPayload artifactPayload, CancellationToken token = default)
        {
            artifactPayload.VerifyNotNull(nameof(artifactPayload));

            ArtifactId artifactId = new ArtifactId(artifactPayload.Id);

            _logger.LogTrace($"{nameof(Set)}: Writing {artifactId}");
            await _dataLakeStore.Write(RealPath(artifactId), artifactPayload.ToBytes(), true, token);
        }

        private string RealPath(ArtifactId artifactId) => RealPath(artifactId.Path);

        private string RealPath(string? path) => _pathRoot + (_pathRoot.IsEmpty() ? string.Empty : "/") + (path ?? string.Empty);

        public string RemovePathRoot(string path)
        {
            string newPath = path.Substring(_pathRoot.Length);
            if (newPath.StartsWith("/")) newPath = newPath.Substring(1);

            return newPath;
        }
    }
}
