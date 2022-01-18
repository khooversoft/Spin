using Azure;
using Directory.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Security;
using Toolbox.Security.Sign;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public class IdentityService
{
    private const string _issuer = "identity.com";
    private const string _audience = "spin.com";
    private readonly IDocumentStorage _documentStorage;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(IDocumentStorage documentStorage, ILogger<IdentityService> logger)
    {
        documentStorage.VerifyNotNull(nameof(documentStorage));

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
            ClassType = "identity",
            Subject = identityEntryRequest.Issuer,
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
        if (directoryEntry == null)
        {
            _logger.LogTrace($"Cannot find entry for directoryId={documentId}");
            return null;
        }

        return directoryEntry with { ETag = eTag };
    }

    public async Task<IdentityEntry> Set(IdentityEntry entry, CancellationToken token = default)
    {
        ETag eTag = await _documentStorage.Set((DocumentId)entry.DirectoryId, entry, entry.ETag, token);

        return entry with { ETag = eTag };
    }

    public Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) => _documentStorage.Search(queryParameter, token);

    public async Task<string?> Sign(SignRequest signRequest, CancellationToken token)
    {
        _logger.LogTrace($"Sign for directoryId={signRequest.DirectoryId}");

        IdentityEntry? identityEntry = await Get((DocumentId)signRequest.DirectoryId, token);
        if (identityEntry == null) return null;

        IPrincipleSignature principleSignature = new PrincipleSignature(signRequest.DirectoryId, _issuer, _audience, identityEntry.Subject, identityEntry.GetRsaParameters());

        return new JwtTokenBuilder()
            .SetDigest(signRequest.Digest)
            .SetPrincipleSignature(principleSignature)
            .SetExpires(DateTime.Now.AddYears(10))
            .SetIssuedAt(DateTime.Now)
            .Build();
    }

    public async Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token)
    {
        _logger.LogTrace($"Validate for directoryId={validateRequest.DirectoryId}");

        IdentityEntry? identityEntry = await Get((DocumentId)validateRequest.DirectoryId, token);
        if (identityEntry == null) return false;

        IPrincipleSignature principleSignature = new PrincipleSignature(validateRequest.DirectoryId, _issuer, _audience, identityEntry.Subject, identityEntry.GetRsaParameters());

        try
        {
            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .SetPrincipleSignature(principleSignature)
                .Build()
                .Parse(validateRequest.Jwt);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed validation for directoryId={validateRequest.DirectoryId}");
            return false;
        }
    }
}
