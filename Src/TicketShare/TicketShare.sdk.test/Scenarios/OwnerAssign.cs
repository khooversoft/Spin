using System.Collections.Frozen;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
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
    private const string _friend1 = "friend1@otherDomain.com";
    private const string _friend2 = "friend2@otherDomain.com";

    private readonly RoleRecord[] _users = [
        new RoleRecord { PrincipalId = _principalId, MemberRole = RoleType.Owner },
        new RoleRecord { PrincipalId = _friend1, MemberRole =RoleType.Contributor },
        new RoleRecord { PrincipalId = _friend2, MemberRole = RoleType.Contributor },
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
        var testHost = new TicketShareTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<OwnerAssign>();

        var accountRecord = TestTool.Create(_principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "samUser", testHost, context);
        await TestTool.AddIdentityUser(_friend1, "friend-user1", testHost, context);
        await TestTool.AddIdentityUser(_friend2, "friend-user2", testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);
        await CreateGroup(testHost, context);
        await AddProposal(testHost, context);

    }

    private async Task<TicketGroupRecord> CreateGroup(TicketShareTestHost testHost, ScopeContext context)
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
        result.IsOk().Should().BeTrue(result.ToString());

        return ticketGroup;
    }

    private async Task AddProposal(TicketShareTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var proposal = new ProposalRecord
        {
            SeatId = "sec1-row1-seat1",
            Proposed = new StateDetail
            {
                Date = DateTime.UtcNow,
                ByPrincipalId = _principalId,
            }
        };

        var ticketGroupOption = await client.Get(_ticketGroupId, context);
        ticketGroupOption.IsOk().Should().BeTrue();

        var ticketGroup = ticketGroupOption.Return();

        ticketGroup = ticketGroup with
        {
            Proposals = ticketGroup.Proposals.Values.Append(proposal).ToFrozenDictionary(x => x.ProposalId, x => x),
        };

        var write = await client.Set(ticketGroup, context);
        write.IsOk().Should().BeTrue();
    }

    public async Task AcceptProposal(TicketShareTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var ticketGroupOption = await client.Get(_ticketGroupId, context);
        ticketGroupOption.IsOk().Should().BeTrue();
    }
}
