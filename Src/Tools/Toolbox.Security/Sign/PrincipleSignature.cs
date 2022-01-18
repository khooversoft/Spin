using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Toolbox.Security.Sign;

public class PrincipleSignature : PrincipleSignatureBase
{
    private RSA _rsa;

    public PrincipleSignature(string kid, string issuer, string? audience, string? subject = null, RSAParameters? rasParameters = null)
        : base(kid, issuer, audience, subject)
    {
        _rsa = rasParameters == null ? RSA.Create() : RSA.Create((RSAParameters)rasParameters!);
    }

    public PrincipleSignature(string kid, string issuer, string? audience, string? subject, PrincipleSignature source)
        : base(kid, issuer, audience, subject)
    {
        _rsa = source._rsa;
    }

    public RSAParameters RSAParameters(bool includePrivateKey) => _rsa.ExportParameters(includePrivateKey);

    public override SigningCredentials GetSigningCredentials() => new SigningCredentials(new RsaSecurityKey(RSAParameters(true)), SecurityAlgorithms.RsaSha512);

    public override SecurityKey GetSecurityKey() => new RsaSecurityKey(RSAParameters(false));
}
