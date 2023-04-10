using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
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

        public IPrincipalSignature? PrincipleSignature { get; set; }

        public IList<Claim> Claims { get; } = new List<Claim>();

        public DateTime? NotBefore { get; set; }

        public DateTime? Expires { get; set; }

        public DateTime? IssuedAt { get; set; }

        public string? Digest { get; set; }


        public JwtTokenBuilder AddClaim(Claim claim)
        {
            claim.NotNull();

            Claims.Add(claim);
            return this;
        }


        public JwtTokenBuilder SetPrincipleSignature(IPrincipalSignature orincipleSignature) => this.Action(x => x.PrincipleSignature = orincipleSignature);

        public JwtTokenBuilder SetNotBefore(DateTime? notBefore) => this.Action(x => x.NotBefore = notBefore);

        public JwtTokenBuilder SetExpires(DateTime? expires) => this.Action(x => x.Expires = expires);

        public JwtTokenBuilder SetIssuedAt(DateTime? issuedAt) => this.Action(x => x.IssuedAt = issuedAt);

        public JwtTokenBuilder SetDigest(string? digest) => this.Action(x => x.Digest = digest);

        public string Build()
        {
            PrincipleSignature.NotNull(name: $"{nameof(PrincipleSignature)} is required");

            var header = new JwtHeader(PrincipleSignature.GetSigningCredentials());
            if (!PrincipleSignature.Kid.IsEmpty()) header["kid"] = PrincipleSignature.Kid;

            var addClaims = new[]
            {
                PrincipleSignature.Subject.IsEmpty() ? null : new Claim(JwtStandardClaimNames.SubjectName, PrincipleSignature.Subject),
                Digest.IsEmpty() ? null : new Claim(JwtStandardClaimNames.DigestName, Digest),
            }.Where(x => x != null);

            var payload = new JwtPayload(PrincipleSignature.Issuer, PrincipleSignature.Audience, Claims.Concat(addClaims), NotBefore, Expires, IssuedAt);

            var jwtToken = new JwtSecurityToken(header, payload);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(jwtToken);
        }
    }
}