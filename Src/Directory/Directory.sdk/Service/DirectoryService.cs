using Azure;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public class DirectoryService
{
    private readonly IDocumentStorage _documentStorage;

    public DirectoryService(IDocumentStorage documentStorage)
    {
        documentStorage.VerifyNotNull(nameof(documentStorage));

        _documentStorage = documentStorage;
    }

    public async Task<DirectoryEntry?> Get(DocumentId documentId, CancellationToken token = default, bool bypassCache = false)
    {
        (DirectoryEntry? directoryEntry, ETag? eTag) = await _documentStorage.Get<DirectoryEntry>(documentId, token, bypassCache);
        if (directoryEntry == null) return null;

        return directoryEntry with { ETag = eTag };
    }

    public async Task<DirectoryEntry> Set(DirectoryEntry entry, CancellationToken token = default)
    {
        ETag eTag = await _documentStorage.Set((DocumentId)entry.DirectoryId, entry, entry.ETag, token);

        return entry with { ETag = eTag };
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token) => await _documentStorage.Delete(documentId, token);

    public Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) => _documentStorage.Search(queryParameter, token);
}
