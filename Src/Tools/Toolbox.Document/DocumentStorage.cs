using Azure;
using Microsoft.Extensions.Caching.Memory;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Toolbox.Document;

public class DocumentStorage : IDocumentStorage
{
    private readonly IDatalakeStore _store;
    private readonly IMemoryCache? _memoryCache;

    public DocumentStorage(IDatalakeStore store)
    {
        _store = store.VerifyNotNull(nameof(store)); ;
    }

    public DocumentStorage(IDatalakeStore store, IMemoryCache memoryCache)
    {
        _store = store.VerifyNotNull(nameof(store)); ;
        _memoryCache = memoryCache;
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token = default)
    {
        _memoryCache?.Remove(GetKey(documentId));

        return await _store.Delete(documentId.ToJsonFileName(), token: token);
    }

    public async Task<(T?, ETag? eTag)> Get<T>(DocumentId documentId, CancellationToken token = default, bool bypassCache = false)
    {
        documentId.VerifyNotNull(nameof(documentId));

        if (_memoryCache != null && !bypassCache)
        {
            if (_memoryCache.TryGetValue<DocumentCache<T>>(GetKey(documentId), out DocumentCache<T> foundEntry)) return (foundEntry.Value, foundEntry.ETag);
            _memoryCache.Remove(GetKey(documentId));
        }

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
        documentId.VerifyNotNull(nameof(documentId));
        value.VerifyNotNull(nameof(value));

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

    private string GetKey(DocumentId documentId) => documentId.ToString().ToLower();

    private record DocumentCache<T>(T Value, ETag? ETag);
}
