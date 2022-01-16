using Azure;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Directory.Test.Application;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Security;
using Toolbox.Security.Keys;
using Toolbox.Security.Services;
using Toolbox.Tools;
using Xunit;

namespace Directory.Test;

public class IdentityControllerTests
{
    [Fact]
    public void GivenNewIdentity_RSAParameters_Match()
    {
        const string issuer = "user@domain.com";
        var documentId = new DocumentId("test/unit-tests-identity/identity1");

        var request = new IdentityEntryRequest
        {
            DirectoryId = (string)documentId,
            Issuer = issuer
        };

        RSA rsa = RSA.Create();
        RSAParameters source = rsa.ExportParameters(true);

        var entry = new IdentityEntry
        {
            DirectoryId = (string)documentId,
            ClassType = "identity",
            Issuer = issuer,
            PublicKey = rsa.ExportRSAPublicKey(),
            PrivateKey = rsa.ExportRSAPrivateKey(),
        };

        RSAParameters result = entry.GetRsaParameters();
        source.D!.Length.Should().Be(result.D!.Length);
        source.DP!.Length.Should().Be(source.DP!.Length);
        source.DQ!.Length.Should().Be(source.DQ!.Length);
        source.Exponent!.Length.Should().Be(source.Exponent!.Length);
        source.InverseQ!.Length.Should().Be(source.InverseQ!.Length);
        source.Modulus!.Length.Should().Be(source.Modulus!.Length);
        source.P!.Length.Should().Be(source.P!.Length);
        source.Q!.Length.Should().Be(source.Q!.Length);

        Enumerable.SequenceEqual(source.D, result.D!);
        Enumerable.SequenceEqual(source.DP, result.DP!);
        Enumerable.SequenceEqual(source.Exponent, result.Exponent!);
        Enumerable.SequenceEqual(source.InverseQ, result.InverseQ!);
        Enumerable.SequenceEqual(source.Modulus, result.Modulus!);
        Enumerable.SequenceEqual(source.P, result.P!);
        Enumerable.SequenceEqual(source.Q, result.Q!);
    }

    [Fact]
    public async Task GivenDirectoryEntry_WhenRoundTrip_Success()
    {
        const string issuer = "user@domain.com";

        IdentityClient client = TestApplication.GetIdentityClient();

        var documentId = new DocumentId("test/unit-tests-identity/identity1");

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-identity",
            Recursive = false,
        };

        IReadOnlyList<DatalakePathItem> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x.Name == (string)documentId)) await client.Delete(documentId);

        var request = new IdentityEntryRequest
        {
            DirectoryId = (string)documentId,
            Issuer = issuer
        };

        bool success = await client.Create(request);
        success.Should().BeTrue();

        IdentityEntry? entry = await client.Get(documentId);
        entry.Should().NotBeNull();

        RSAParameters rSAParameters = entry!.GetRsaParameters();

        IKeyService keyService = new KeyServiceBuilder()
            .Add(entry!.Issuer, new RsaPublicKey(rSAParameters))
            .Build();

        IPrincipleSignatureCollection signatureCollection = new PrincipleSignatureCollection()
            .Add(new PrincipleSignature(issuer, "business@domain.com", keyService));

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleSignature(signatureCollection[issuer])
            .Build();

        var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };

        blockChain.Add(payload, signatureCollection[issuer]);
        blockChain.Validate(signatureCollection);

        await client.Delete(documentId);
        search = (await client.Search(query).ReadNext()).Records;
        search.Any(x => x.Name == (string)documentId).Should().BeFalse();
    }

    //[Fact]
    //public async Task GivenDirectoryEntry_WhenRoundTripWithETag_Success()
    //{
    //    DirectoryClient client = TestApplication.GetDirectoryClient();

    //    var documentId = new DocumentId("test/unit-tests/entry1");

    //    var query = new QueryParameter()
    //    {
    //        Filter = "test",
    //        Recursive = false,
    //    };

    //    await client.Delete(documentId);

    //    var entry = new DirectoryEntryBuilder()
    //        .SetDirectoryId(documentId)
    //        .SetClassType("test")
    //        .AddProperty(new EntryProperty { Name = "property1", Value = "value1" })
    //        .Build();

    //    await client.Set(entry);

    //    DirectoryEntry? readEntry = await client.Get(documentId);
    //    readEntry.Should().NotBeNull();
    //    readEntry!.ETag.Should().NotBeNull();
    //    readEntry.Properties.Count.Should().Be(1);

    //    var updateEntry = new DirectoryEntryBuilder(readEntry)
    //        .SetClassType("test-next")
    //        .AddProperty(new EntryProperty { Name = "property2", Value = "value2" })
    //        .Build();

    //    await client.Set(updateEntry);

    //    readEntry = await client.Get(documentId);
    //    readEntry.Should().NotBeNull();
    //    readEntry!.Properties.Count.Should().Be(2);

    //    await client.Delete(documentId);
    //    IReadOnlyList<DatalakePathItem> search = (await client.Search(query).ReadNext()).Records;
    //    search.Any(x => x.Name == (string)documentId).Should().BeFalse();
    //}


    //[Fact]
    //public async Task GivenDirectoryEntry_WhenRoundTripWithETag_Fail()
    //{
    //    DirectoryClient client = TestApplication.GetDirectoryClient();

    //    var documentId = new DocumentId("test/unit-tests/entry1");

    //    var query = new QueryParameter()
    //    {
    //        Filter = "test",
    //        Recursive = false,
    //    };

    //    await client.Delete(documentId);

    //    var entry = new DirectoryEntryBuilder()
    //        .SetDirectoryId(documentId)
    //        .SetClassType("test")
    //        .AddProperty(new EntryProperty { Name = "property1", Value = "value1" })
    //        .Build();

    //    await client.Set(entry);

    //    DirectoryEntry? readEntry = await client.Get(documentId);
    //    readEntry.Should().NotBeNull();
    //    readEntry!.ETag.Should().NotBeNull();
    //    readEntry.Properties.Count.Should().Be(1);

    //    var updateEntry = new DirectoryEntryBuilder(readEntry)
    //        .SetClassType("test-next")
    //        .SetETag(new ETag("0xFF9CA90CB9F5120"))
    //        .AddProperty(new EntryProperty { Name = "property2", Value = "value2" })
    //        .Build();

    //    bool failed;
    //    try
    //    {
    //        await client.Set(updateEntry);
    //        failed = false;
    //    }
    //    catch(Azure.RequestFailedException)
    //    {
    //        failed = true;
    //    }

    //    failed.Should().BeTrue();

    //    await client.Delete(documentId);
    //    IReadOnlyList<DatalakePathItem> search = (await client.Search(query).ReadNext()).Records;
    //    search.Any(x => x.Name == (string)documentId).Should().BeFalse();
    //}

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }
}
