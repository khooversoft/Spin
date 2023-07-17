﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;

namespace SpinCluster.sdk.test.Tools;

public class RsaTests
{
    [Fact]
    public void TestSigningAndValidateOfJwtWithRSA()
    {
        string digest = "this is a digest";

        var rsa = RSA.Create();
        RSAParameters rSAParameters = rsa.ExportParameters(true);

        byte[] pubk = rsa.ExportRSAPublicKey();
        byte[] privk = rsa.ExportRSAPrivateKey();


        // Sign
        var signRsa = RSA.Create();
        signRsa.ImportRSAPrivateKey(privk, out int _);

        RSAParameters signParameter = signRsa.ExportParameters(true);
        var signature = new PrincipalSignature("kid", "issuer", "audience", rasParameters: signParameter);

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(signature)
            .Build();

        token.Should().NotBeNull();

        // Verify
        var validateRsa = RSA.Create();
        validateRsa.ImportRSAPublicKey(pubk, out int _);

        RSAParameters validateParameter = validateRsa.ExportParameters(false);
        var validate = new PrincipalSignature("kid", "issuer", "audience", rasParameters: validateParameter);

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(validate)
            .Build()
            .Parse(token);

        tokenDetails.Digest.Should().Be(digest);
    }

    [Fact]
    public void TestSigningAndValidateOfJwtWithPrincipalSignature()
    {
        string digest = "this is a digest";

        var rsa = RSA.Create();
        RSAParameters rSAParameters = rsa.ExportParameters(true);

        byte[] pubk = rsa.ExportRSAPublicKey();
        byte[] privk = rsa.ExportRSAPrivateKey();


        // Sign
        var signature = PrincipalSignature.CreateFromPrivateKeyOnly(privk, "kid", "issuer", "audience");

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(signature)
            .Build();

        token.Should().NotBeNull();

        // Verify
        var validate = PrincipalSignature.CreateFromPublicKeyOnly(pubk, "kid", "issuer", "audience");

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(validate)
            .Build()
            .Parse(token);

        tokenDetails.Digest.Should().Be(digest);
    }
}
