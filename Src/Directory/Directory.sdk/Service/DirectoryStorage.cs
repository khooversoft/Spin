using Azure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Directory.sdk.Service;

internal class DirectoryStorage
{
    private readonly IDatalakeStore _store;

    public DirectoryStorage(IDatalakeStore store)
    {
        _store = store.VerifyNotNull(nameof(store)); ;
    }

    public async Task<ETag> Set(DirectoryEntry directoryEntry, CancellationToken token)
    {
        directoryEntry.Verify();

        string path = ((DirectoryId)directoryEntry.DirectoryId).ToFileName();
        ETag? eTag = directoryEntry.ETag;
        directoryEntry = directoryEntry with { ETag = null };

        return await _store.Write(path, directoryEntry.ToJsonFormat().ToBytes(), true, eTag: eTag, token: token);
    }

    public async Task<DirectoryEntry?> Get(DirectoryId directoryId, CancellationToken token = default)
    {
        string path = directoryId.ToFileName();
        (byte[] Data, ETag eTag) = await _store.ReadWithTag(path, token);

        DirectoryEntry? entry = Json.Default.Deserialize<DirectoryEntry>(Data.BytesToString());
        if (entry == null) return null;

        entry = entry with { ETag = eTag };
        entry.Verify();
        return entry;
    }

    public async Task Delete(DirectoryId directoryId, CancellationToken token = default) => await _store.Delete(directoryId.ToFileName(), token: token);

    public async Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default)
    {
        IReadOnlyList<DatalakePathItem> list = await _store.Search(queryParameter, token);

        return list
            .Select(x => x with { Name = DirectoryIdUtility.FromFileName(x.Name) })
            .ToList();
    }

    public async Task<DatalakePathProperties> GetProperty(DirectoryId directoryId, CancellationToken token = default) => await _store.GetPathProperties(directoryId.ToFileName(), token);
}
