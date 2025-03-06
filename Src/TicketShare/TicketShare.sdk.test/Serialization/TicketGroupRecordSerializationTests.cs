using Toolbox.Extensions;
using Toolbox.Tools.Should;

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
        const string ticketGroupId = "sam/2020/hockey";
        const string _friend1 = "friend1@otherDomain.com";
        const string _friend2 = "friend2@otherDomain.com";

        var rec1 = new TicketGroupRecord
        {
            TicketGroupId = ticketGroupId,
            Name = "name",
            Description = "Sam's 2020 hockey tickets",

            Roles = [
                    new RoleRecord { PrincipalId = principalId, MemberRole = RoleType.Owner },
                ],

            Seats = [
                    new SeatRecord { Section = "1", Row = "10", Seat = "Sec-5-Row-7-Seat-8", AssignedToPrincipalId = _friend1 },
                    new SeatRecord { Section = "1", Row = "10", Seat = "Sec-5-Row-7-Seat-9", AssignedToPrincipalId = _friend2 },
                ],

            Proposals = new Dictionary<string, ProposalRecord>()
            {
                ["proposal1"] = new ProposalRecord
                {
                    ProposalId = "proposal1",
                    SeatId = "Sec-5-Row-7-Seat-9",
                    Proposed = new StateDetail
                    {
                        Date = DateTime.UtcNow,
                        ByPrincipalId = principalId,
                    },
                },
                ["proposal2"] = new ProposalRecord
                {
                    ProposalId = "proposal2",
                    SeatId = "Sec-5-Row-7-Seat-8",
                    Proposed = new StateDetail
                    {
                        Date = DateTime.UtcNow.AddDays(-1),
                        ByPrincipalId = principalId,
                    },
                },
            },
        };

        var json = rec1.ToJson();
        var rec2 = json.ToObject<TicketGroupRecord>();

        (rec1 == rec2).Should().BeTrue();
    }
}
