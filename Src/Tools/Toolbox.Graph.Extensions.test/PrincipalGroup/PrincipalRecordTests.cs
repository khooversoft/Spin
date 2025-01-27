using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.PrincipalGroup;

public class PrincipalRecordTests
{
    [Fact]
    public void RoundTrip()
    {
        var m1 = new SecurityGroupRecord
        {
            SecurityGroupId = "group1",
            Name = "group1",
            Members = new Dictionary<string, MemberAccessRecord>
            {
                ["user1"] = new MemberAccessRecord { PrincipalId = "user1", Access = PrincipalAccess.Read },
                ["user2"] = new MemberAccessRecord { PrincipalId = "user2", Access = PrincipalAccess.Contributor },
            }
        };

        m1.Validate().IsOk().Should().BeTrue();

        string json = m1.ToJson();
        json.NotEmpty();

        var m2 = json.ToObject<SecurityGroupRecord>();
        m2.NotNull();

        (m1 == m2).Should().BeTrue();
    }
}
