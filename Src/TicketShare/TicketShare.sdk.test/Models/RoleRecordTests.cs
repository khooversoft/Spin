using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.test.Models;

public class RoleRecordTests
{
    [Fact]
    public void Serialization1()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RoleType.Owner,
        };

        p1.Validate().IsOk().BeTrue();
        string j1 = p1.ToJson();
        var p2 = j1.ToObject<RoleRecord>();
        (p1 == p2).BeTrue();
    }

    [Fact]
    public void Compare1()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RoleType.Owner,
        };

        var p2 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RoleType.Owner,
        };

        (p1 == p2).BeTrue();
    }

    [Fact]
    public void NegCompare1()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RoleType.Contributor,
        };

        var p2 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RoleType.Owner,
        };

        (p1 == p2).BeFalse();
    }

    [Fact]
    public void NegCompare2()
    {
        var p1 = new RoleRecord
        {
            PrincipalId = "key2",
            MemberRole = RoleType.Owner,
        };

        var p2 = new RoleRecord
        {
            PrincipalId = "key1",
            MemberRole = RoleType.Owner,
        };

        (p1 == p2).BeFalse();
    }
}
