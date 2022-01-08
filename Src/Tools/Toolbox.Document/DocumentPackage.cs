using Azure;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Toolbox.Tools;
using Toolbox.Tools.Zip;

namespace Toolbox.Document;

public class DocumentPackage : IDocumentPackage
{
    private readonly IDatalakeStore _store;
    private const string _defaultPath = "default.package";

    public DocumentPackage(IDatalakeStore store)
    {
        _store = store.VerifyNotNull(nameof(store));
    }

    public Task<bool> Delete(DocumentId id, CancellationToken token = default) => _store.Delete(id.ToZipFileName(), token: token);

    public async Task<Document?> Get(DocumentId id, CancellationToken token = default)
    {
        id.VerifyNotNull(nameof(id));

        byte[]? data = await _store.Read(id.ToZipFileName(), token);
        if (data == null) return null;

        using var dataStream = new MemoryStream(data);
        using var zip = new ZipArchive(dataStream, ZipArchiveMode.Read, leaveOpen: true);

        byte[] defaultPackage = zip.Read(_defaultPath);
        return Json.Default.Deserialize<Document>(Encoding.UTF8.GetString(defaultPackage));
    }

    public async Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) =>
        (await _store.Search(queryParameter, token))
            .Select(x => x with { Name = DocumentIdTools.RemoveExtension(x.Name) })
            .ToList();

    public async Task<ETag> Set(Document document, ETag? eTag = null, CancellationToken token = default)
    {
        document.VerifyNotNull(nameof(document));

        string json = Json.Default.Serialize(document);
        byte[] data = Encoding.UTF8.GetBytes(json);

        using var writeBuffer = new MemoryStream();
        using (var zipWrite = new ZipArchive(writeBuffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            zipWrite.Write(_defaultPath, data);
        }

        writeBuffer.Seek(0, SeekOrigin.Begin);
        return await _store.Write(document.DocumentId.ToZipFileName(), writeBuffer.ToArray(), true, eTag, token);
    }
}
