using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.SecurityGroup;

public class SecurityAccessTests
{
    [Theory]
    [InlineData(SecurityAccess.None, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.None, SecurityAccess.Reader, false)]
    [InlineData(SecurityAccess.None, SecurityAccess.Contributor, false)]
    [InlineData(SecurityAccess.None, SecurityAccess.Owner, false)]

    [InlineData(SecurityAccess.Reader, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.Reader, SecurityAccess.Reader, true)]
    [InlineData(SecurityAccess.Reader, SecurityAccess.Contributor, false)]
    [InlineData(SecurityAccess.Reader, SecurityAccess.Owner, false)]

    [InlineData(SecurityAccess.Contributor, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.Contributor, SecurityAccess.Reader, true)]
    [InlineData(SecurityAccess.Contributor, SecurityAccess.Contributor, true)]
    [InlineData(SecurityAccess.Contributor, SecurityAccess.Owner, false)]

    [InlineData(SecurityAccess.Owner, SecurityAccess.None, true)]
    [InlineData(SecurityAccess.Owner, SecurityAccess.Reader, true)]
    [InlineData(SecurityAccess.Owner, SecurityAccess.Contributor, true)]
    [InlineData(SecurityAccess.Owner, SecurityAccess.Owner, true)]

    public void TestNoAccess(SecurityAccess access, SecurityAccess requireAccess, bool shouldPass)
    {
        var result = access.HasAccess(requireAccess).IsOk();
        result.Be(shouldPass);
    }
}
