using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Security.Keys;
using Toolbox.Security.Services;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Security.Test
{
    public class JwtTests
    {
        private const string jwtKeyId = "94B9AC01699678F7115DAD10FF022D4C400836F0";
        //private const string vaultKeyId = "E994953A89D53B1F4E5B2529B8DD702F38E8A889";

        private static readonly LocalCertificate _jwtVaultTest =
            new LocalCertificate(StoreLocation.LocalMachine, StoreName.My, "94B9AC01699678F7115DAD10FF022D4C400836F0", true, new NullLogger<LocalCertificate>());

        private static readonly LocalCertificate _vaultData =
            new LocalCertificate(StoreLocation.LocalMachine, StoreName.My, "E994953A89D53B1F4E5B2529B8DD702F38E8A889", true, new NullLogger<LocalCertificate>());

        public static IKeyService _keyService = new KeyServiceBuilder()
            .Add(jwtKeyId, _jwtVaultTest.GetCertificate())
            .Build();

        public static IKeyService _vaultkeyService = new KeyServiceBuilder()
            .Add(jwtKeyId, _vaultData.GetCertificate())
            .Build();


        [Fact]
        public void VerifyRsaSerialization()
        {
            using RSA rsa = RSA.Create();

            RSAParameters publicKey = rsa.ExportParameters(includePrivateParameters: true);
            byte[] pubk = rsa.ExportRSAPublicKey();
            byte[] privk = rsa.ExportRSAPrivateKey();

            string xml = rsa.ToXmlString(includePrivateParameters: true);
            string xml1 = rsa.ToXmlString(includePrivateParameters: true);

            using RSA rsa1 = RSA.Create();
            rsa1.FromXmlString(xml);

            RSAParameters publicKey2 = rsa1.ExportParameters(includePrivateParameters: false);
            RSAParameters privateKey2 = rsa1.ExportParameters(includePrivateParameters: true);

            string xml21 = rsa1.ToXmlString(includePrivateParameters: true);
        }

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
                .SetKeyService(_keyService)
                .SetKeyId(jwtKeyId)
                .Build();

            token.Should().NotBeNullOrEmpty();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .SetKeyService(_keyService)
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
                .SetKeyService(_keyService)
                .SetKeyId(jwtKeyId)
                .Build();

            token.Should().NotBeNullOrEmpty();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .SetKeyService(_keyService)
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
                .SetKeyService(_keyService)
                .SetKeyId(jwtKeyId)
                .Build();

            token.Should().NotBeNullOrEmpty();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .SetKeyService(_keyService)
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
                .SetKeyService(_keyService)
                .SetKeyId(jwtKeyId)
                .Build();

            token.Should().NotBeNullOrEmpty();

            Action act = () => new JwtTokenParserBuilder()
                .SetKeyService(_vaultkeyService)
                .AddValidIssuer(issuer.ToString())
                .Build()
                .Parse(token);

            // Failure
            act.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();

            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .SetKeyService(_keyService)
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
                .SetKeyService(_keyService)
                .SetKeyId(jwtKeyId)
                .Build();

            token.Should().NotBeNullOrEmpty();

            token = token.Remove(3, 2);

            Action act = () => new JwtTokenParserBuilder()
                .SetKeyService(_vaultkeyService)
                .AddValidIssuer(issuer.ToString())
                .Build()
                .Parse(token);

            // Failure
            act.Should().Throw<ArgumentException>();
        }
    }
}