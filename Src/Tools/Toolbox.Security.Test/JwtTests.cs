using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Toolbox.Security.Test
{
    public class JwtTests
    {
        private static readonly LocalCertificate _jwtVaultTest =
            new LocalCertificate(StoreLocation.LocalMachine, StoreName.My, "94B9AC01699678F7115DAD10FF022D4C400836F0", true, new NullLogger<LocalCertificate>());

        private static readonly LocalCertificate _vaultData =
            new LocalCertificate(StoreLocation.LocalMachine, StoreName.My, "E994953A89D53B1F4E5B2529B8DD702F38E8A889", true, new NullLogger<LocalCertificate>());

        [Trait("Category", "LocalOnly")]
        [Fact]
        public void JwtSecurityTokenBuilderTest()
        {
            const string userId = "user@domain.com";
            const string emailText = "Email";
            const string emailId = "testemail@domain.com";

            string token = new JwtTokenBuilder()
                .SetAudience(new Uri("http://localhost/audience").ToString())
                .SetIssuer(new Uri("http://localhost/Issuer").ToString())
                .SetExpires(DateTime.Now.AddDays(10))
                .SetIssuedAt(DateTime.Now)
                .SetClaim(new Claim(emailText, emailId))
                .AddSubject(userId)
                .SetCertificate(_jwtVaultTest.GetCertificate())
                .Build();

            token.Should().NotBeNullOrEmpty();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .AddCertificate(_jwtVaultTest.GetCertificate())
                .Build()
                .Parse(token);

            tokenDetails.JwtSecurityToken.Header.Kid.Should().Be(_jwtVaultTest.LocalCertificateKey.Thumbprint);
            tokenDetails.JwtSecurityToken.Subject.Should().Be(userId);
            tokenDetails.JwtSecurityToken.Claims.Any(x => x.Type == emailText && x.Value == emailId).Should().BeTrue();
        }

        [Trait("Category", "LocalOnly")]
        [Fact]
        public void JwtSecurityTokenAudienceTest()
        {
            var audience = new Uri("http://localhost/audience");

            string token = new JwtTokenBuilder()
                .SetAudience(audience.ToString())
                .SetExpires(DateTime.Now.AddDays(10))
                .SetIssuedAt(DateTime.Now)
                .SetClaim(new Claim("Email", "testemail@domain.com"))
                .SetCertificate(_jwtVaultTest.GetCertificate())
                .Build();

            token.Should().NotBeNullOrEmpty();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .AddCertificate(_jwtVaultTest.GetCertificate())
                .AddValidAudience(audience.ToString())
                .Build()
                .Parse(token);

            tokenDetails.JwtSecurityToken.Header.Kid.Should().Be(_jwtVaultTest.LocalCertificateKey.Thumbprint);
        }

        [Trait("Category", "LocalOnly")]
        [Fact]
        public void JwtSecurityTokenIssuerTest()
        {
            var issuer = new Uri("http://localhost/Issuer");

            string token = new JwtTokenBuilder()
                .SetIssuer(issuer.ToString())
                .SetExpires(DateTime.Now.AddDays(10))
                .SetIssuedAt(DateTime.Now)
                .SetClaim(new Claim("Email", "testemail@domain.com"))
                .SetCertificate(_jwtVaultTest.GetCertificate())
                .Build();

            token.Should().NotBeNullOrEmpty();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .AddCertificate(_jwtVaultTest.GetCertificate())
                .AddValidIssuer(issuer.ToString())
                .Build()
                .Parse(token);

            tokenDetails.JwtSecurityToken.Header.Kid.Should().Be(_jwtVaultTest.LocalCertificateKey.Thumbprint);
        }

        [Trait("Category", "LocalOnly")]
        [Fact]
        public void JwtSecurityFailureTest()
        {
            var issuer = new Uri("http://localhost/Issuer");

            string token = new JwtTokenBuilder()
                .SetIssuer(issuer.ToString())
                .SetExpires(DateTime.Now.AddDays(10))
                .SetIssuedAt(DateTime.Now)
                .SetClaim(new Claim("Email", "testemail@domain.com"))
                .SetCertificate(_jwtVaultTest.GetCertificate())
                .Build();

            token.Should().NotBeNullOrEmpty();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .AddCertificate(_vaultData.GetCertificate())
                .AddValidIssuer(issuer.ToString())
                .Build()
                .Parse(token);

            // Failure
            tokenDetails.Should().BeNull();

            tokenDetails = new JwtTokenParserBuilder()
                .AddCertificate(_jwtVaultTest.GetCertificate())
                .AddCertificate(_vaultData.GetCertificate())
                .AddValidIssuer(issuer.ToString())
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
                .SetIssuer(issuer.ToString())
                .SetExpires(DateTime.Now.AddDays(10))
                .SetIssuedAt(DateTime.Now)
                .SetClaim(new Claim("Email", "testemail@domain.com"))
                .SetCertificate(_jwtVaultTest.GetCertificate())
                .Build();

            token.Should().NotBeNullOrEmpty();

            token = token.Remove(3, 2);

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .AddCertificate(_jwtVaultTest.GetCertificate())
                .AddValidIssuer(issuer.ToString())
                .Build()
                .Parse(token);

            tokenDetails.Should().BeNull();
        }
    }
}