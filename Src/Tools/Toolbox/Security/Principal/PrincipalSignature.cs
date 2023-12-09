using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Toolbox.Tools;

namespace Toolbox.Security;

public class PrincipalSignature : PrincipalSignatureBase
{
    private RSA _rsa;

    public PrincipalSignature(string kid, string issuer, string? audience, string? subject = null, RSAParameters? rasParameters = null, DateTime? expires = null)
        : base(kid, issuer, audience, subject, expires)
    {
        _rsa = rasParameters switch
        {
            null => RSA.Create(),
            RSAParameters v => RSA.Create(v),
        };
    }

    public RSAParameters RSAParameters(bool includePrivateKey) => _rsa.ExportParameters(includePrivateKey);

    public override SigningCredentials GetSigningCredentials() => new SigningCredentials(new RsaSecurityKey(RSAParameters(true)), SecurityAlgorithms.RsaSha512);

    public override SecurityKey GetSecurityKey() => new RsaSecurityKey(RSAParameters(false));

    public static PrincipalSignature CreateFromPrivateKeyOnly(byte[] privateKey, string kid, string issuer, string? audience, string? subject = null, DateTime? expires = null)
    {
        privateKey.NotNull();

        var signRsa = RSA.Create();
        signRsa.ImportRSAPrivateKey(privateKey, out int _);

        RSAParameters signParameter = signRsa.ExportParameters(true);
        return new PrincipalSignature(kid, issuer, audience, subject, signParameter, expires);
    }

    public static PrincipalSignature CreateFromPublicKeyOnly(byte[] publicKey, string kid, string issuer, string? audience, string? subject = null, DateTime? expires = null)
    {
        publicKey.NotNull();

        var validateRsa = RSA.Create();
        validateRsa.ImportRSAPublicKey(publicKey, out int _);

        RSAParameters validateParameter = validateRsa.ExportParameters(false);
        return new PrincipalSignature(kid, issuer, audience, subject, validateParameter, expires);
    }
}
