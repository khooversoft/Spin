using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Directory.Test.Application;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Security.Sign;
using Xunit;

namespace Directory.Test;

public class IdentityControllerTests
{
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

        var issuerSignature = new PrincipalSignature(issuer, issuer, "business@domain.com", rasParameters: entry!.GetRsaParameters());

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(issuer)
            .Build()
            .Sign(x => issuerSignature);

        var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };

        blockChain.Add(payload, issuer);
        blockChain.Validate(x => issuerSignature);

        await client.Delete(documentId);
        search = (await client.Search(query).ReadNext()).Records;
        search.Any(x => x.Name == (string)documentId).Should().BeFalse();
    }

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }
}
