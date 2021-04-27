using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;

namespace Toolbox.Azure.DataLake
{
    public interface IDataLakeStore
    {
        Task Append(string path, byte[] data, CancellationToken token);

        Task<bool> Delete(string path, CancellationToken token);

        Task DeleteDirectory(string path, CancellationToken token);

        Task Download(string path, Stream toStream, CancellationToken token);

        Task<bool> Exist(string path, CancellationToken token);

        Task<DatalakePathProperties> GetPathProperties(string path, CancellationToken token);

        Task<byte[]> Read(string path, CancellationToken token);

        Task<IReadOnlyList<DataLakePathItem>> Search(QueryParameter queryParameter, Func<DataLakePathItem, bool> filter, bool recursive, CancellationToken token);

        Task Upload(Stream fromStream, string toPath, bool force, CancellationToken token);

        Task Write(string path, byte[] data, bool force, CancellationToken token);
    }
}