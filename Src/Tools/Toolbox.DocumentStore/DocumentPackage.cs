using Azure;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Toolbox.Tools;
using Toolbox.Tools.Zip;

namespace Toolbox.DocumentStore;

public class DocumentPackage
{
    private readonly IDatalakeStore _store;
    private readonly ILogger<DocumentPackage> _logger;
    private const string _defaultPath = "default.package";

    public DocumentPackage(IDatalakeStore store, ILogger<DocumentPackage> logger)
    {
        _store = store.VerifyNotNull(nameof(store));
        _logger = logger;
    }

    public Task<bool> Delete(DocumentId id, CancellationToken token = default) => _store.Delete(id.ToZipFileName(), token: token);

    public async Task<Document?> Get(DocumentId id, CancellationToken token = default)
    {
        id.VerifyNotNull(nameof(id));
        string zipFileName = id.ToZipFileName();

        _logger.LogTrace($"Reading {zipFileName}");
        byte[]? data = await _store.Read(zipFileName, token);
        if (data == null)
        {
            _logger.LogTrace($"File {zipFileName} not found");
            return null;
        }

        using var dataStream = new MemoryStream(data);
        using var zip = new ZipArchive(dataStream, ZipArchiveMode.Read, leaveOpen: true);

        byte[] defaultPackage = zip.Read(_defaultPath);
        Document? document = Json.Default.Deserialize<Document>(Encoding.UTF8.GetString(defaultPackage));
        if(document == null)
        {
            _logger.LogWarning($"File {zipFileName} cannot be deserialize into {nameof(Document)}");
            return null;
        }

        _logger.LogTrace($"File {zipFileName} read and {nameof(Document)} returned");
        return document;
    }

    public async Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) =>
        (await _store.Search(queryParameter, token))
            .Select(x => x with { Name = DocumentIdTools.RemoveExtension(x.Name) })
            .ToList();

    public async Task<ETag> Set(Document document, ETag? eTag = null, CancellationToken token = default)
    {
        document.Verify();
        string zipFileName = document.DocumentId.ToZipFileName();

        _logger.LogTrace($"Writing documentId={document.DocumentId} to {zipFileName}");

        string json = Json.Default.Serialize(document);
        byte[] data = Encoding.UTF8.GetBytes(json);

        using var writeBuffer = new MemoryStream();
        using (var zipWrite = new ZipArchive(writeBuffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            zipWrite.Write(_defaultPath, data);
        }

        writeBuffer.Seek(0, SeekOrigin.Begin);
        ETag resultEtag = await _store.Write(document.DocumentId.ToZipFileName(), writeBuffer.ToArray(), true, eTag, token);

        _logger.LogTrace($"DocumentId={document.DocumentId} written to {zipFileName}");
        return resultEtag;
    }
}
