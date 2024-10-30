using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Types;

namespace TicketShare.sdk.test.Scenarios;

/// <summary>
/// Owner creates ticket group
/// Add users wtih contributor role
/// Creates proposal and send to users
/// All users agree
/// Proposal is applied
/// </summary>
public class OwnerAssign
{
    private const string _principalId = "sam@domain.com";
    private const string _ticketGroupId = "sam/2020/hockey";
    private const string _proposalId = "sam/initial-proposal";

    private readonly RoleRecord[] _users = [
        new RoleRecord { PrincipalId = _principalId, MemberRole = RoleType.Owner },
        new RoleRecord { PrincipalId = "friend1@otherDomain.com", MemberRole =RoleType.Contributor },
        new RoleRecord { PrincipalId = "friend2@farDomain.com", MemberRole = RoleType.Contributor },
        ];

    private readonly SeatRecord[] _seats = [
        new SeatRecord { SeatId = "sec1-row1-seat1", Date = new DateTime(2024, 1, 1) },
        new SeatRecord { SeatId = "sec1-row1-seat2", Date = new DateTime(2024, 1, 1) },
        new SeatRecord { SeatId = "sec1-row1-seat1", Date = new DateTime(2024, 1, 10) },
        new SeatRecord { SeatId = "sec1-row1-seat2", Date = new DateTime(2024, 1, 10) },
    ];

    [Fact]
    public async Task OwnerAssignUserConfirm()
    {
        var testHost = new TestHost();
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<OwnerAssign>();
        const string principalId = "user1@domain.com";

        var accountRecord = TestTool.Create(principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);
        await CreateGroup(testHost, context);
        await CreateProposal(testHost, context);


    }

    private async Task<TicketGroupRecord> CreateGroup(TestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var ticketGroup = new TicketGroupRecord
        {
            TicketGroupId = _ticketGroupId,
            Name = "Ticket Group Name",
            Description = "Sam's 2020 hockey tickets",
            OwnerPrincipalId = _principalId,

            Roles = _users,
            Seats = _seats,
        };

        var option = ticketGroup.Validate();
        option.IsOk().Should().BeTrue();

        var result = await client.Add(ticketGroup, context);
        result.IsOk().Should().BeTrue();

        return ticketGroup;
    }

    private async Task<ProposalRecord> CreateProposal(TestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<ProposalClient>();

        var subject = new ProposalRecord
        {
            ProposalId = _proposalId,
            State = ProposalState.Open,
            TicketGroupId = _ticketGroupId,
            AuthorPrincipalId = _principalId,
            Description = "Initial proposal",
            ProposedDate = DateTime.UtcNow,
            Seats = _seats
                .Select(x => new ProposalSeatRecord
                {
                    TicketGroupId = _ticketGroupId,
                    SeatId = x.SeatId,
                    Date = x.Date
                }).ToArray(),
        };

        var option = subject.Validate();
        option.IsOk().Should().BeTrue();

        var result = await client.Add(subject, context);
        result.IsOk().Should().BeTrue();

        return subject;
    }
}
