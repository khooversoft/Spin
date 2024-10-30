using FluentAssertions;
using Toolbox.Extensions;

namespace TicketShare.sdk.test.Schema;

public class TicketGroupRecordSerializationTests
{
    [Fact]
    public void DefaultTestEqual()
    {
        var n1 = new TicketGroupRecord();
        var n2 = new TicketGroupRecord();
        (n1 == n2).Should().BeTrue();
    }

    [Fact]
    public void EqualTest()
    {
        const string principalId = "user1@domain.com";

        var rec1 = new TicketGroupRecord
        {
            TicketGroupId = "sam/2020/hockey",
            Name = "name",
            Description = "Sam's 2020 hockey tickets",
            OwnerPrincipalId = principalId,

            Roles = [
                new RoleRecord { PrincipalId = principalId, MemberRole = RoleType.Owner },
                ],

            Seats = [
                new SeatRecord { SeatId = "Sec-5-Row-7-Seat-8", AssignedToPrincipalId = principalId },
                new SeatRecord { SeatId = "Sec-5-Row-7-Seat-9", AssignedToPrincipalId = principalId },
                ],
        };

        var json = rec1.ToJson();
        var rec2 = json.ToObject<TicketGroupRecord>();

        (rec1 == rec2).Should().BeTrue();
    }
}
