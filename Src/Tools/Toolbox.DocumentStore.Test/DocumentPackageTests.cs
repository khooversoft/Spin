using Azure;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Xunit;

namespace Toolbox.DocumentStore.Test;

public class DocumentPackageTests
{
    [Fact]
    public async Task GivenDocument_WhenPackage_WillRoundTrip()
    {
        var payload = new Payload
        {
            Name = "name",
            Description = "description",
        };

        DocumentId documentId = (DocumentId)"test/path";

        var store = new Store();
        var package = new DocumentPackage(store, new NullLogger<DocumentPackage>());

        var document = new DocumentBuilder()
            .SetData(payload)
            .SetDocumentId(documentId)
            .Build();

        await package.Set(document);
        store._package.Should().NotBeNull();

        Document readDocument = (await package.Get(documentId)).NotNull();

        (document == readDocument).Should().BeTrue();
    }


    private record Payload
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }

    private class Store : IDatalakeStore
    {
        public byte[]? _package;

        public Task Append(string path, byte[] data, CancellationToken token = default) => throw new NotImplementedException();

        public Task<bool> Delete(string path, ETag? eTag = null, CancellationToken token = default) => throw new NotImplementedException();

        public Task DeleteDirectory(string path, CancellationToken token = default) => throw new NotImplementedException();

        public Task<bool> Exist(string path, CancellationToken token = default) => throw new NotImplementedException();

        public Task<DatalakePathProperties> GetPathProperties(string path, CancellationToken token = default) => throw new NotImplementedException();

        public Task<byte[]?> Read(string path, CancellationToken token = default)
        {
            _package.Should().NotBeNull();
            return Task.FromResult(_package);
        }

        public Task Read(string path, Stream toStream, CancellationToken token = default) => throw new NotImplementedException();

        public Task<(byte[]? Data, ETag? Etag)> ReadWithTag(string path, CancellationToken token = default) => throw new NotImplementedException();

        public Task<IReadOnlyList<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token = default) => throw new NotImplementedException();

        public Task<ETag> Write(Stream fromStream, string toPath, bool overwrite, ETag? eTag = null, CancellationToken token = default) => throw new NotImplementedException();

        public Task<ETag> Write(string path, byte[] data, bool overwrite, ETag? eTag = null, CancellationToken token = default)
        {
            _package = data.ToArray();
            return Task.FromResult(new ETag("default"));
        }
    }
}
