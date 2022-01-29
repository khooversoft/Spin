using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Security.Sign;
using Xunit;

namespace Toolbox.Security.Test;

public class JwtCertificateTests
{
    private const string jwtKeyId = "94B9AC01699678F7115DAD10FF022D4C400836F0";
    //private const string vaultKeyId = "E994953A89D53B1F4E5B2529B8DD702F38E8A889";

    private static readonly LocalCertificate _jwtVaultTest =
        new LocalCertificate(StoreLocation.LocalMachine, StoreName.My, "94B9AC01699678F7115DAD10FF022D4C400836F0", true, new NullLogger<LocalCertificate>());

    private static readonly LocalCertificate _vaultData =
        new LocalCertificate(StoreLocation.LocalMachine, StoreName.My, "E994953A89D53B1F4E5B2529B8DD702F38E8A889", true, new NullLogger<LocalCertificate>());

    public static IPrincipalSignature _jwtPrincipleSignature = new PrincipalSignatureCertificate(jwtKeyId, "test.com", "test.com", _jwtVaultTest.GetCertificate());
    public static IPrincipalSignature _vaultPrincipleSignature = new PrincipalSignatureCertificate(jwtKeyId, "test.com", "test.com", _vaultData.GetCertificate());


    [Trait("Category", "LocalOnly")]
    [Fact]
    public void JwtSecurityTokenBuilderTest()
    {
        const string userId = "user@domain.com";
        const string emailText = "Email";
        const string emailId = "testemail@domain.com";

        IPrincipalSignature principle = _jwtPrincipleSignature.WithSubject(userId);

        string token = new JwtTokenBuilder()
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .AddClaim(new Claim(emailText, emailId))
            .SetPrincipleSignature(principle)
            .Build();

        token.Should().NotBeNullOrEmpty();

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build()
            .Parse(token);

        tokenDetails.JwtSecurityToken.Header.Kid.Should().Be(jwtKeyId);
        tokenDetails.JwtSecurityToken.Subject.Should().Be(userId);
        tokenDetails.JwtSecurityToken.Claims.Any(x => x.Type == emailText && x.Value == emailId).Should().BeTrue();
    }

    [Trait("Category", "LocalOnly")]
    [Fact]
    public void JwtSecurityTokenAudienceTest()
    {
        string audience = "http://localhost/audience";

        IPrincipalSignature principle = _jwtPrincipleSignature.WithAudience(audience);

        string token = new JwtTokenBuilder()
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .AddClaim(new Claim("Email", "testemail@domain.com"))
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build();

        token.Should().NotBeNullOrEmpty();

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build()
            .Parse(token);
    }

    [Trait("Category", "LocalOnly")]
    [Fact]
    public void JwtSecurityTokenIssuerTest()
    {
        string issuer = "http://localhost/Issuer";

        IPrincipalSignature principle = _jwtPrincipleSignature.WithIssuer(issuer);

        string token = new JwtTokenBuilder()
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .AddClaim(new Claim("Email", "testemail@domain.com"))
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build();

        token.Should().NotBeNullOrEmpty();

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build()
            .Parse(token);
    }

    [Trait("Category", "LocalOnly")]
    [Fact]
    public void JwtSecurityFailureTest()
    {
        string token = new JwtTokenBuilder()
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .AddClaim(new Claim("Email", "testemail@domain.com"))
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build();

        token.Should().NotBeNullOrEmpty();

        IPrincipalSignature principle = _vaultPrincipleSignature.WithIssuer("bad issuer");

        Action act = () => new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle)
            .Build()
            .Parse(token);

        // Failure
        act.Should().Throw<SecurityTokenUnableToValidateException>();

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build()
            .Parse(token);

        tokenDetails.Should().NotBeNull();
        tokenDetails.JwtSecurityToken.Header.Kid.Should().Be(_jwtVaultTest.LocalCertificateKey.Thumbprint);
    }

    [Trait("Category", "LocalOnly")]
    [Fact]
    public void JwtSecuritySignatureFailureTest()
    {
        var issuer = new Uri("http://localhost/Issuer");

        string token = new JwtTokenBuilder()
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .AddClaim(new Claim("Email", "testemail@domain.com"))
            .SetPrincipleSignature(_jwtPrincipleSignature)
            .Build();

        token.Should().NotBeNullOrEmpty();

        token = token.Remove(3, 2);

        Action act = () => new JwtTokenParserBuilder()
            .SetPrincipleSignature(_vaultPrincipleSignature)
            .Build()
            .Parse(token);

        // Failure
        act.Should().Throw<ArgumentException>();
    }
}
