using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;

namespace Toolbox.Security
{
    /// <summary>
    /// Build a JWT token parser, specify certificates, audiences, and issuers.  This is just a helper builder pattern class
    /// </summary>
    public class JwtTokenParserBuilder
    {
        public IPrincipalSignature? PrincipleSignature { get; set; }

        public IList<string?> ValidAudiences { get; } = new List<string?>();

        public IList<string?> ValidIssuers { get; set; } = new List<string?>();

        public JwtTokenParserBuilder AddValidAudience(params string?[] validAudience) => this.Action(x => validAudience.ForEach(y => ValidAudiences.Add(y)));

        public JwtTokenParserBuilder AddValidIssuer(params string[] validIssuer) => this.Action(x => validIssuer.ForEach(y => ValidIssuers.Add(y)));

        public JwtTokenParserBuilder SetPrincipleSignature(IPrincipalSignature orincipleSignature) => this.Action(x => x.PrincipleSignature = orincipleSignature);

        public JwtTokenParser Build()
        {
            PrincipleSignature.NotNull($"{nameof(PrincipleSignature)} is required");

            return new JwtTokenParser(PrincipleSignature, ValidIssuers, ValidAudiences);
        }
    }
}