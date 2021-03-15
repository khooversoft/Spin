using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Extensions;
using Toolbox.Security.Services;
using Toolbox.Tools;

namespace Toolbox.Security
{
    /// <summary>
    /// Build JWT token (builder pattern)
    /// </summary>
    public class JwtTokenBuilder
    {
        public JwtTokenBuilder()
        {
        }

        public IKeyService? KeyService { get; set; }

        public string? KeyId { get; set; }

        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public IList<Claim> Claims { get; } = new List<Claim>();

        public DateTime? NotBefore { get; set; }

        public DateTime? Expires { get; set; }

        public DateTime? IssuedAt { get; set; }

        public string? WebKey { get; set; }

        public JwtTokenBuilder AddSubject(string subject)
        {
            subject.VerifyNotNull(nameof(subject));

            Claims.Add(new Claim(JwtStandardClaimNames.SubjectName, subject));
            return this;
        }

        public JwtTokenBuilder SetDigest(string payloadDigest)
        {
            payloadDigest.VerifyNotNull(nameof(payloadDigest));

            Claims.Add(new Claim(JwtStandardClaimNames.DigestName, payloadDigest));
            return this;
        }

        public JwtTokenBuilder SetKeyService(IKeyService keyService) => this.Action(x => x.KeyService = keyService);

        public JwtTokenBuilder SetKeyId(string keyId) => this.Action(x => x.KeyId = keyId);

        public JwtTokenBuilder SetIssuer(string issuer) => this.Action(x => x.Issuer = issuer);

        public JwtTokenBuilder SetAudience(string audience) => this.Action(x => x.Audience = audience);

        public JwtTokenBuilder SetNotBefore(DateTime? notBefore) => this.Action(x => x.NotBefore = notBefore);

        public JwtTokenBuilder SetExpires(DateTime? expires) => this.Action(x => x.Expires = expires);

        public JwtTokenBuilder SetIssuedAt(DateTime? issuedAt) => this.Action(x => x.IssuedAt = issuedAt);

        public JwtTokenBuilder SetClaim(Claim claim)
        {
            claim.VerifyNotNull(nameof(claim));

            Claims.Add(claim);
            return this;
        }

        public JwtTokenBuilder SetWebKey(string webKey) => this.Action(x => x.WebKey = webKey);

        public string Build()
        {
            KeyService.VerifyNotNull($"{nameof(KeyService)} is required");
            KeyId.VerifyNotEmpty($"{nameof(KeyId)} is required");

            SigningCredentials signingCredentials = KeyService.GetSigningCredentials(KeyId)
                .VerifyNotNull($"{KeyId} found in KeyService");

            var header = new JwtHeader(signingCredentials);
            header["kid"] = KeyId;

            var addClaims = new List<Claim>();
            if (!WebKey.IsEmpty())
            {
                addClaims.Add(new Claim(JwtStandardClaimNames.WebKeyName, WebKey));
            };

            var payload = new JwtPayload(Issuer, Audience, Claims.Concat(addClaims), NotBefore, Expires, IssuedAt);

            var jwtToken = new JwtSecurityToken(header, payload);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(jwtToken);
        }
    }
}