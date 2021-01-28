using Microsoft.Extensions.Logging.Abstractions;
using System;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security
{
    public class PrincipleSignature : IPrincipleSignature
    {
        public PrincipleSignature(string issuer, string audience, TimeSpan validFor, RsaPublicPrivateKey publicPrivateKey)
        {
            issuer.VerifyNotEmpty(nameof(issuer));
            publicPrivateKey.VerifyNotNull(nameof(publicPrivateKey));

            Issuer = issuer;
            Audience = audience;
            ValidFor = validFor;
            PublicPrivateKey = publicPrivateKey;
        }

        public string Issuer { get; }

        public string Audience { get; }

        public TimeSpan ValidFor { get; }

        public RsaPublicPrivateKey PublicPrivateKey { get; }

        public string Sign(string payloadDigest)
        {
            return new JwtTokenBuilder()
                .SetDigest(payloadDigest)
                .SetIssuer(Issuer)
                .SetAudience(Audience)
                .SetExpires(DateTime.UtcNow.Add(ValidFor))
                .SetPrivateKey(PublicPrivateKey)
                .Build();
        }

        public JwtTokenDetails? ValidateSignature(string jwt)
        {
            return new JwtTokenParser(PublicPrivateKey.ToEnumerable(), Issuer.ToEnumerable(), Audience.ToEnumerable(), new NullLogger<JwtTokenParser>())
                .Parse(jwt);
        }
    }
}