using Microsoft.Extensions.Logging.Abstractions;
using System;
using Toolbox.Extensions;
using Toolbox.Security.Services;
using Toolbox.Tools;

namespace Toolbox.Security
{
    public class PrincipleSignature : IPrincipleSignature
    {
        private readonly IKeyService _keyService;

        public PrincipleSignature(string issuer, string audience, IKeyService keyService)
        {
            issuer.VerifyNotEmpty(nameof(issuer));
            keyService.VerifyNotNull(nameof(keyService));

            Issuer = issuer;
            Audience = audience;
            _keyService = keyService;
        }

        public string Issuer { get; }

        public string Audience { get; }

        public string Sign(string payloadDigest)
        {
            return new JwtTokenBuilder()
                .SetDigest(payloadDigest)
                .SetIssuer(Issuer)
                .SetAudience(Audience)
                .SetKeyService(_keyService)
                .SetKeyId(Issuer)
                .SetExpires(DateTime.Now.AddYears(10))
                .SetIssuedAt(DateTime.Now)
                .Build();
        }

        public JwtTokenDetails ValidateSignature(string jwt)
        {
            return new JwtTokenParserBuilder()
                .SetKeyService(_keyService)
                .AddValidIssuer(Issuer)
                .AddValidAudience(Audience)
                .Build()
                .Parse(jwt);
        }
    }
}