using System.Security.Cryptography;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.Test.Security;

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

        token.NotNull();

        // Verify
        var validateRsa = RSA.Create();
        validateRsa.ImportRSAPublicKey(pubk, out int _);

        RSAParameters validateParameter = validateRsa.ExportParameters(false);
        var validate = new PrincipalSignature("kid", "issuer", "audience", rasParameters: validateParameter);

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(validate)
            .Build()
            .Parse(token);

        tokenDetails.Digest.Be(digest);
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

        token.NotNull();

        // Verify
        var validate = PrincipalSignature.CreateFromPublicKeyOnly(pubk, "kid", "issuer", "audience");

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(validate)
            .Build()
            .Parse(token);

        tokenDetails.Digest.Be(digest);
    }
}
