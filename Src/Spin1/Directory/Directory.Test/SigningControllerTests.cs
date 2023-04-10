using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Directory.Test.Application;
using FluentAssertions;
using Spin.Common.Model;
using Spin.Common.Sign;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block.Application;
using Toolbox.Block.Signature;
using Toolbox.Model;
using Xunit;

namespace Directory.Test;

public class SigningControllerTests
{
    [Fact]
    public async Task GivenIdentityEntry_WhenSigned_WillVerify()
    {
        const string issuer = "user@domain.com";

        IdentityClient client = TestApplication.GetIdentityClient();
        SigningClient signClient = TestApplication.GetSigningClient();

        var documentId = new DocumentId("identity:test/SigningControllerTests/identity1");

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-identity",
            Recursive = false,
        };

        IReadOnlyList<DatalakePathItem> search = (await client.Search(query).ReadNext()).Records;
        bool isInsearch = search.Any(x => x.Name == documentId.Path);

        bool deleted = await client.Delete(documentId);
        (isInsearch == deleted).Should().BeTrue();

        var request = new IdentityEntryRequest
        {
            DirectoryId = (string)documentId,
            Issuer = issuer
        };

        bool success = await client.Create(request);
        success.Should().BeTrue();

        var signRequest = new SignRequest
        {
            PrincipleDigests = new[]
            {
                new PrincipleDigest
                {
                    PrincipleId = (string)documentId,
                    Digest = Guid.NewGuid().ToString()
                }
            }
        };

        SignRequestResponse signedJwt = await signClient.Sign(signRequest);
        signedJwt.Should().NotBeNull();
        (signedJwt.Errors == null || signedJwt.Errors.Count == 0).Should().BeTrue();
        signedJwt.PrincipleDigests.Count.Should().Be(1);

        var validateRequest = new ValidateRequest
        {
            PrincipleDigests = new[]
            {
                new PrincipleDigest
                {
                    PrincipleId = (string)documentId,
                    Digest = signRequest.PrincipleDigests[0].Digest,
                    JwtSignature = signedJwt.PrincipleDigests.First().JwtSignature,
                }
            }
        };

        bool jwtValidated = await signClient.Validate(validateRequest);
        jwtValidated.Should().BeTrue();

        await client.Delete(documentId);
        search = (await client.Search(query).ReadNext()).Records;
        search.Any(x => x.Name == (string)documentId).Should().BeFalse();
    }
}
