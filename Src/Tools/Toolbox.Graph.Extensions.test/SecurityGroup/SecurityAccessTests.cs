using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.SecurityGroup;

public class SecurityAccessTests
{
    [Theory]
    [InlineData(SecurityAccess.None, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.None, SecurityAccess.Read, false)]
    [InlineData(SecurityAccess.None, SecurityAccess.Contributor, false)]
    [InlineData(SecurityAccess.None, SecurityAccess.Owner, false)]

    [InlineData(SecurityAccess.Read, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.Read, SecurityAccess.Read, true)]
    [InlineData(SecurityAccess.Read, SecurityAccess.Contributor, false)]
    [InlineData(SecurityAccess.Read, SecurityAccess.Owner, false)]

    [InlineData(SecurityAccess.Contributor, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.Contributor, SecurityAccess.Read, true)]
    [InlineData(SecurityAccess.Contributor, SecurityAccess.Contributor, true)]
    [InlineData(SecurityAccess.Contributor, SecurityAccess.Owner, false)]

    [InlineData(SecurityAccess.Owner, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.Owner, SecurityAccess.Read, true)]
    [InlineData(SecurityAccess.Owner, SecurityAccess.Contributor, true)]
    [InlineData(SecurityAccess.Owner, SecurityAccess.Owner, true)]

    public void TestNoAccess(SecurityAccess access, SecurityAccess requireAccess, bool shouldPass)
    {
        var result = access.HasAccess(requireAccess).IsOk();
        result.Should().Be(shouldPass);
    }
}
