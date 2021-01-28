using System;

namespace Toolbox.Security
{
    public interface IPrincipleSignature
    {
        string Audience { get; }

        string Issuer { get; }

        RsaPublicPrivateKey PublicPrivateKey { get; }

        TimeSpan ValidFor { get; }

        string Sign(string payloadDigest);

        JwtTokenDetails? ValidateSignature(string jwt);
    }
}