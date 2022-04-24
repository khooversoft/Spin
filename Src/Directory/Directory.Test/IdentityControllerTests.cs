using Directory.sdk.Client;
using Directory.sdk.Service;
using Directory.Test.Application;
using FluentAssertions;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Model;
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

        await client.Delete(documentId);

        var request = new IdentityEntryRequest
        {
            DirectoryId = (string)documentId,
            Issuer = issuer
        };

        bool success = await client.Create(request);
        success.Should().BeTrue();

        IdentityEntry? entry = await client.Get(documentId);
        entry.Should().NotBeNull();

        await client.Delete(documentId);
    }

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }
}
