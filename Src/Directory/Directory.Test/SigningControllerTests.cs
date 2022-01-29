using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Directory.Test.Application;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Xunit;

namespace Directory.Test;

public class SigningControllerTests
{
    [Fact]
    public async Task GivenDirectoryEntry_WhenSigned_WillVerify()
    {
        const string issuer = "user@domain.com";

        IdentityClient client = TestApplication.GetIdentityClient();
        SigningClient signClient = TestApplication.GetSigningClient();

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

        var signedJwt = await signClient.Sign(signRequest);

        var validateRequest = new ValidateRequest
        {
            DirectoryId = (string)(documentId),
            Jwt = signedJwt,
        };

        bool jwtValidated = await signClient.Validate(validateRequest);
        jwtValidated.Should().BeTrue();


        await client.Delete(documentId);
        search = (await client.Search(query).ReadNext()).Records;
        search.Any(x => x.Name == (string)documentId).Should().BeFalse();
    }
}
