using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;

namespace Directory.sdk.Service
{
    public interface IDirectoryService
    {
        Task Delete(DirectoryId directoryId, CancellationToken token);
        Task<DirectoryEntry?> Get(DirectoryId directoryId, CancellationToken token = default, bool bypassCache = false);
        Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default);
        Task Set(DirectoryEntry entry, CancellationToken token = default);
    }
}