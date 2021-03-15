using System;

namespace Toolbox.Security
{
    public interface IPrincipleSignature
    {
        string Issuer { get; }

        string Sign(string payloadDigest);

        JwtTokenDetails? ValidateSignature(string jwt);
    }
}