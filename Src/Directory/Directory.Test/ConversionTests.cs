using Directory.sdk.Service;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Security;
using Toolbox.Security.Sign;
using Xunit;

namespace Directory.Test;

public class ConversionTests
{
    [Fact]
    public void GivenNewIdentity_RSAParameters_Match()
    {
        const string issuer = "user@domain.com";
        var documentId = new DocumentId("test/unit-tests-identity/identity1");

        RSA rsa = RSA.Create();
        RSAParameters source = rsa.ExportParameters(true);

        var entry = new IdentityEntry
        {
            DirectoryId = (string)documentId,
            Subject = issuer,
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
    public void GivenIdentity_WhenSigned_ShouldValidate()
    {
        var documentId = new DocumentId("test/unit-tests-identity/identity1");

        string kid = Guid.NewGuid().ToString();
        string subject = "email@test.com";
        string digest = Guid.NewGuid().ToString();

        RSAParameters rsaParameter = Create(documentId);

        IPrincipalSignature principle = new PrincipalSignature(kid, "test.com", "spin.com", subject, rsaParameter);

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(principle)
            .Build();

        token.Should().NotBeNullOrEmpty();

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle)
            .Build()
            .Parse(token);

        tokenDetails.JwtSecurityToken.Header.Kid.Should().Be(kid);
        tokenDetails.JwtSecurityToken.Subject.Should().Be(subject);
    }


    [Fact]
    public void GivenIdentity_WhenSigned_ShouldFailWithOtherIdentity()
    {
        var documentId = new DocumentId("test/unit-tests-identity/identity1");
        var documentId2 = new DocumentId("test/unit-tests-identity/identity2");

        string kid = Guid.NewGuid().ToString();
        string subject = "email@test.com";
        string digest = Guid.NewGuid().ToString();

        RSAParameters rsaParameter = Create(documentId);
        RSAParameters rsaParameter2 = Create(documentId2);

        IPrincipalSignature principle = new PrincipalSignature(kid, "test.com", "spin.com", subject, rsaParameter);
        IPrincipalSignature principle2 = new PrincipalSignature(kid, "test.com", "spin.com", subject, rsaParameter2);

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(principle)
            .Build();

        token.Should().NotBeNullOrEmpty();

        Action test = () => new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle2)
            .Build()
            .Parse(token);

        test.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();
    }

    private RSAParameters Create(DocumentId documentId)
    {
        RSA rsa = RSA.Create();
        RSAParameters source = rsa.ExportParameters(true);

        var entry = new IdentityEntry
        {
            DirectoryId = (string)documentId,
            Subject = "<subject>",
            PublicKey = rsa.ExportRSAPublicKey(),
            PrivateKey = rsa.ExportRSAPrivateKey(),
        };

        return entry.GetRsaParameters();
    }
}
