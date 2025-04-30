using Microsoft.IdentityModel.Tokens;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.Test.Security;

public class JwtKeyTests
{
    [Fact]
    public void GivenJwtSigned_ShouldValidate()
    {
        string kid = Guid.NewGuid().ToString();
        string subject = "email@test.com";
        string digest = Guid.NewGuid().ToString();

        IPrincipalSignature principle = new PrincipalSignature(kid, "test.com", "spin.com", subject);

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(principle)
            .Build();

        token.NotEmpty();

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle)
            .Build()
            .Parse(token);

        tokenDetails.JwtSecurityToken.Header.Kid.Be(kid);
        tokenDetails.JwtSecurityToken.Subject.Be(subject);
    }

    [Fact]
    public void GivenJwtSigned_WhenAnotherPrincipleIsUsed_ShouldFailValidate()
    {
        string kid = Guid.NewGuid().ToString();
        string kid2 = Guid.NewGuid().ToString();
        string subject = "email@test.com";
        string digest = Guid.NewGuid().ToString();

        IPrincipalSignature principle = new PrincipalSignature(kid, "test.com", "spin.com", subject);
        IPrincipalSignature principle2 = new PrincipalSignature(kid2, "test2.com", "spin2.com", subject);

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(principle)
            .Build();

        token.NotEmpty();

        Verify.Throw<SecurityTokenValidationException>(() =>
        {
            new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle2)
            .Build()
            .Parse(token);
        });
    }
}
