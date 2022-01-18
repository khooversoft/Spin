using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Security.Sign;
using Xunit;

namespace Toolbox.Security.Test;

public class JwtKeyTests
{
    [Fact]
    public void GivenJwtSigned_ShouldValidate()
    {
        string kid = Guid.NewGuid().ToString();
        string subject = "email@test.com";
        string digest = Guid.NewGuid().ToString();

        IPrincipleSignature principle = new PrincipleSignature(kid, "test.com", "spin.com", subject);

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(principle)
            .Build();

        token.Should().NotBeNullOrEmpty();

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

        IPrincipleSignature principle = new PrincipleSignature(kid, "test.com", "spin.com", subject);
        IPrincipleSignature principle2 = new PrincipleSignature(kid2, "test2.com", "spin2.com", subject);

        string token = new JwtTokenBuilder()
            .SetDigest(digest)
            .SetExpires(DateTime.Now.AddDays(10))
            .SetIssuedAt(DateTime.Now)
            .SetPrincipleSignature(principle)
            .Build();

        token.Should().NotBeNullOrEmpty();

        Action test = () => new JwtTokenParserBuilder()
            .SetPrincipleSignature(principle2)
            .Build()
            .Parse(token);

        test.Should().Throw<SecurityTokenUnableToValidateException>();
    }
}
