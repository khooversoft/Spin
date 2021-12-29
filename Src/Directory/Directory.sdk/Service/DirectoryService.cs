using Azure;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public class DirectoryService : IDirectoryService
{
    private readonly DirectoryStorage _storage;
    private readonly IMemoryCache _memoryCache;
    private static readonly MemoryCacheEntryOptions _cachePolicy = new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

    public DirectoryService(IDatalakeStore dataLakeStore, IMemoryCache memoryCache)
    {
        dataLakeStore.VerifyNotNull(nameof(dataLakeStore));
        memoryCache.VerifyNotNull(nameof(memoryCache));

        _storage = new DirectoryStorage(dataLakeStore);
        _memoryCache = memoryCache;
    }

    public async Task<DirectoryEntry?> Get(DirectoryId directoryId, CancellationToken token = default, bool bypassCache = false)
    {
        if (!bypassCache && _memoryCache.TryGetValue<DirectoryEntry>(GetKey(directoryId), out DirectoryEntry foundEntry)) return foundEntry;

        DirectoryEntry? entry = await _storage.Get(directoryId, token);
        if (entry == null) return null;

        _memoryCache.Set(GetKey(directoryId), entry, _cachePolicy);
        return entry;
    }

    public async Task Set(DirectoryEntry entry, CancellationToken token = default)
    {
        entry.VerifyNotNull(nameof(entry));

        ETag eTag = await _storage.Set(entry, token);
        entry = entry with { ETag = eTag };

        _memoryCache.Set(entry.DirectoryId.ToLower(), entry);
    }

    public async Task Delete(DirectoryId directoryId, CancellationToken token)
    {
        _memoryCache.Remove(GetKey(directoryId));

        await _storage.Delete(directoryId, token);
    }

    public Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) => _storage.Search(queryParameter, token);

    private string GetKey(DirectoryId directoryId) => directoryId.ToString().ToLower();
}
