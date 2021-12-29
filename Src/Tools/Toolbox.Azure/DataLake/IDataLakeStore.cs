using Azure;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;

namespace Toolbox.Azure.DataLake
{
    public interface IDatalakeStore
    {
        Task Append(string path, byte[] data, CancellationToken token = default);
        Task<bool> Delete(string path, ETag? eTag = null, CancellationToken token = default);
        Task DeleteDirectory(string path, CancellationToken token = default);
        Task<bool> Exist(string path, CancellationToken token = default);
        Task<DatalakePathProperties> GetPathProperties(string path, CancellationToken token = default);
        Task<byte[]> Read(string path, CancellationToken token = default);
        Task Read(string path, Stream toStream, CancellationToken token = default);
        Task<(byte[] Data, ETag Etag)> ReadWithTag(string path, CancellationToken token = default);
        Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default);
        Task<ETag> Write(Stream fromStream, string toPath, bool overwrite, ETag? eTag = null, CancellationToken token = default);
        Task<ETag> Write(string path, byte[] data, bool overwrite, ETag? eTag = null, CancellationToken token = default);
    }
}