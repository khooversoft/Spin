using Azure;
using Microsoft.Extensions.Caching.Memory;
using Toolbox.Abstractions.Extensions;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;

namespace Toolbox.DocumentStore;

public class DocumentStorage
{
    private readonly IDatalakeStore _store;
    private readonly IMemoryCache? _memoryCache;

    public DocumentStorage(IDatalakeStore store)
    {
        _store = store.NotNull(); ;
    }

    public DocumentStorage(IDatalakeStore store, IMemoryCache memoryCache)
    {
        _store = store.NotNull(); ;
        _memoryCache = memoryCache;
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token = default)
    {
        _memoryCache?.Remove(GetKey(documentId));

        return await _store.Delete(documentId.ToJsonFileName(), token: token);
    }

    public async Task<(T?, ETag? eTag)> Get<T>(DocumentId documentId, CancellationToken token = default, bool bypassCache = false)
    {
        documentId.NotNull();

        if (_memoryCache != null && !bypassCache)
        {
            if (_memoryCache.TryGetValue<DocumentCache<T>>(GetKey(documentId), out DocumentCache<T>? foundEntry)) return (foundEntry!.Value, foundEntry.ETag);
        }

        _memoryCache?.Remove(GetKey(documentId));

        string path = documentId.ToJsonFileName();
        (byte[]? Data, ETag? eTag) = await _store.ReadWithTag(path, token);
        if (Data == null) return (default, null);

        T? entry = Json.Default.Deserialize<T>(Data.BytesToString());
        if (entry == null) return (default, null);

        _memoryCache?.Set(GetKey(documentId), new DocumentCache<T>(entry, eTag));

        return (entry, eTag);
    }

    public async Task<ETag> Set<T>(DocumentId documentId, T value, ETag? eTag = null, CancellationToken token = default)
    {
        documentId.NotNull();
        value.NotNull();

        string path = documentId.ToJsonFileName();
        ETag writeEtag = await _store.Write(path, value.ToJsonFormat().ToBytes(), true, eTag: eTag, token: token);

        _memoryCache?.Set(GetKey(documentId), new DocumentCache<T>(value, writeEtag));

        return writeEtag;
    }

    public async Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default)
    {
        IReadOnlyList<DatalakePathItem> list = await _store.Search(queryParameter, token);

        return list
            .Select(x => x with { Name = DocumentIdTools.RemoveExtension(x.Name) })
            .ToList();
    }

    public async Task<DatalakePathProperties> GetProperty(DocumentId documentId, CancellationToken token = default) => await _store.GetPathProperties(documentId.ToJsonFileName(), token);

    private string GetKey(DocumentId documentId) => documentId.Path.ToLower();

    private record DocumentCache<T>(T Value, ETag? ETag);
}
