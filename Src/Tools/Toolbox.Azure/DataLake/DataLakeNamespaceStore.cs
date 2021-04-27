using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Toolbox.Azure.DataLake
{
    public class DataLakeNamespaceStore : IDataLakeStore
    {
        private readonly DataLakeNamespace _dataLakeNamespace;
        private readonly IDataLakeStore _dataLakeStore;

        public DataLakeNamespaceStore(DataLakeNamespace dataLakeNamespace, IDataLakeStore dataLakeStore)
        {
            dataLakeNamespace.VerifyNotNull(nameof(dataLakeNamespace));
            dataLakeStore.VerifyNotNull(nameof(dataLakeStore));

            _dataLakeNamespace = dataLakeNamespace;
            _dataLakeStore = dataLakeStore;
        }

        public Task Append(string path, byte[] data, CancellationToken token) => _dataLakeStore.Append(WithRootPath(path), data, token);

        public Task<bool> Delete(string path, CancellationToken token) => _dataLakeStore.Delete(WithRootPath(path), token);

        public Task DeleteDirectory(string path, CancellationToken token) => _dataLakeStore.DeleteDirectory(WithRootPath(path), token);

        public Task Download(string path, Stream toStream, CancellationToken token) => _dataLakeStore.Download(WithRootPath(path), toStream, token);

        public Task<bool> Exist(string path, CancellationToken token) => _dataLakeStore.Exist(WithRootPath(path), token);

        public Task<DatalakePathProperties> GetPathProperties(string path, CancellationToken token) => _dataLakeStore.GetPathProperties(WithRootPath(path), token);

        public Task<byte[]> Read(string path, CancellationToken token) => _dataLakeStore.Read(WithRootPath(path), token);

        public async Task<IReadOnlyList<DataLakePathItem>> Search(QueryParameter queryParameter, Func<DataLakePathItem, bool> filter, bool recursive, CancellationToken token)
        {
            IReadOnlyList<DataLakePathItem> list = await _dataLakeStore.Search(
                queryParameter with { Filter = WithRootPath(queryParameter.Filter) },
                filter,
                recursive,
                token);

            return list
                .Select(x => x with { Name = RemovePathRoot(x.Name) })
                .ToList();
        }

        public Task Upload(Stream fromStream, string toPath, bool force, CancellationToken token) => _dataLakeStore.Upload(fromStream, WithRootPath(toPath), force, token);

        public Task Write(string path, byte[] data, bool force, CancellationToken token) => _dataLakeStore.Write(WithRootPath(path), data, force, token);

        private string WithRootPath(string? path) => _dataLakeNamespace.PathRoot + (_dataLakeNamespace.PathRoot.IsEmpty() ? string.Empty : "/") + (path ?? string.Empty);

        private string RemovePathRoot(string path)
        {
            string newPath = path.Substring(_dataLakeNamespace.PathRoot?.Length ?? 0);
            if (newPath.StartsWith("/")) newPath = newPath.Substring(1);

            return newPath;
        }
    }
}