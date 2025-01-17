using Microsoft.IdentityModel.Tokens;
using Toolbox.Security;
using Toolbox.Tools.Should;

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

        token.Should().NotBeEmpty();

        JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle)
            .Build()
            .Parse(token);

        tokenDetails.JwtSecurityToken.Header.Kid.Should().Be(kid);
        tokenDetails.JwtSecurityToken.Subject.Should().Be(subject);
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

        token.Should().NotBeEmpty();

        Action test = () => new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle2)
            .Build()
            .Parse(token);

        test.Should().Throw<SecurityTokenValidationException>();
    }
}
