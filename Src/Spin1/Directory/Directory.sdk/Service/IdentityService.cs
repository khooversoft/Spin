using Azure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions.Models;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.DataLake.Model;
using Toolbox.DocumentStore;

namespace Directory.sdk.Service;

public class IdentityService
{
    private readonly DocumentStorage _documentStorage;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(DocumentStorage documentStorage, ILogger<IdentityService> logger)
    {
        documentStorage.NotNull();

        _documentStorage = documentStorage;
        _logger = logger;
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
            Subject = identityEntryRequest.Issuer,
            PublicKey = rsa.ExportRSAPublicKey(),
            PrivateKey = rsa.ExportRSAPrivateKey(),
        };

        await _documentStorage.Set(documentId, document, token: token);
        return true;
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token) => await _documentStorage.Delete(documentId, token);

    public async Task<IdentityEntry?> Get(DocumentId documentId, CancellationToken token = default, bool bypassCache = false, bool includePrivateKey = false)
    {
        (IdentityEntry? directoryEntry, ETag? eTag) = await _documentStorage.Get<IdentityEntry>(documentId, token, bypassCache);
        if (directoryEntry == null)
        {
            _logger.LogTrace($"Cannot find entry for directoryId={documentId}");
            return null;
        }

        directoryEntry = includePrivateKey ? directoryEntry : directoryEntry with { PrivateKey = null };

        return directoryEntry with { ETag = eTag };
    }

    public async Task<IdentityEntry> Set(IdentityEntry entry, CancellationToken token = default)
    {
        ETag eTag = await _documentStorage.Set((DocumentId)entry.DirectoryId, entry, entry.ETag, token);

        return entry with { ETag = eTag };
    }

    public Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) => _documentStorage.Search(queryParameter, token);
}
