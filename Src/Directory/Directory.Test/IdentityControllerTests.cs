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

        var issuerSignature = new PrincipleSignature(issuer, issuer, "business@domain.com", rasParameters: entry!.GetRsaParameters());

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleSignature(issuerSignature)
            .Build();

        var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };

        blockChain.Add(payload, issuerSignature);
        blockChain.Validate(x => issuerSignature);

        await client.Delete(documentId);
        search = (await client.Search(query).ReadNext()).Records;
        search.Any(x => x.Name == (string)documentId).Should().BeFalse();
    }


    [Fact]
    public async Task GivenDirectoryEntry_WhenSigned_WillVerify()
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

        var signRequest = new SignRequest
        {
            DirectoryId = (string)(documentId),
            Digest = Guid.NewGuid().ToString(),
        };

        var signedJwt = await client.Sign(signRequest);

        var validateRequest = new ValidateRequest
        {
            DirectoryId = (string)(documentId),
            Jwt = signedJwt,
        };

        bool jwtValidated = await client.Validate(validateRequest);
        jwtValidated.Should().BeTrue();


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
