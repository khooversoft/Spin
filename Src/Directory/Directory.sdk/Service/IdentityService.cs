using Azure;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public class IdentityService
{
    private readonly IDocumentStorage _documentStorage;

    public IdentityService(IDocumentStorage documentStorage)
    {
        documentStorage.VerifyNotNull(nameof(documentStorage));

        _documentStorage = documentStorage;
    }

    public async Task<bool> Create(IdentityEntryRequest identityEntryRequest, CancellationToken token)
    {
        identityEntryRequest.Verify();
        DocumentId documentId = new DocumentId(identityEntryRequest.DirectoryId);

        IdentityEntry? exist = await Get(documentId, token: token, bypassCache: true);
        if (exist != null) return false;

        RSA rsa = RSA.Create();

        var document = new IdentityEntry
        {
            DirectoryId = identityEntryRequest.DirectoryId,
            ClassType = "identity",
            Issuer = identityEntryRequest.Issuer,
            PublicKey = rsa.ExportRSAPublicKey(),
            PrivateKey = rsa.ExportRSAPrivateKey(),
        };

        await _documentStorage.Set(documentId, document, token: token);
        return true;
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token) => await _documentStorage.Delete(documentId, token);

    public async Task<IdentityEntry?> Get(DocumentId documentId, CancellationToken token = default, bool bypassCache = false)
    {
        (IdentityEntry? directoryEntry, ETag? eTag) = await _documentStorage.Get<IdentityEntry>(documentId, token, bypassCache);
        if (directoryEntry == null) return null;

        return directoryEntry with { ETag = eTag };
    }

    public async Task<IdentityEntry> Set(IdentityEntry entry, CancellationToken token = default)
    {
        ETag eTag = await _documentStorage.Set((DocumentId)entry.DirectoryId, entry, entry.ETag, token);

        return entry with { ETag = eTag };
    }

    public Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) => _documentStorage.Search(queryParameter, token);
}
