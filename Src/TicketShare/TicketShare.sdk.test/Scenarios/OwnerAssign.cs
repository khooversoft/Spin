using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Tools;
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
        new SeatRecord { SeatId = "sec1-row1-seat1", Date = new DateOnly(2024, 1, 1) },
        new SeatRecord { SeatId = "sec1-row1-seat2", Date = new DateOnly(2024, 1, 1) },
        new SeatRecord { SeatId = "sec1-row1-seat1", Date = new DateOnly(2024, 1, 10) },
        new SeatRecord { SeatId = "sec1-row1-seat2", Date = new DateOnly(2024, 1, 10) },
    ];

    [Fact]
    public async Task OwnerAssignUserConfirm()
    {
        var testHost = new TicketShareTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<OwnerAssign>();

        var accountRecord = TestTool.CreateAccountModel(_principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "samUser", testHost, context);
        await TestTool.AddIdentityUser(_friend1, "friend-user1", testHost, context);
        await TestTool.AddIdentityUser(_friend2, "friend-user2", testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);
        var ticketGroup = await CreateGroup(testHost, context);
        var proposalRecord = await AddProposal(testHost, context);

        await AcceptProposal(ticketGroup.TicketGroupId, proposalRecord.ProposalId, _friend1, testHost, context);
    }

    [Fact]
    public async Task OwnerAssignUserRejects()
    {
        var testHost = new TicketShareTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<OwnerAssign>();

        var accountRecord = TestTool.CreateAccountModel(_principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "samUser", testHost, context);
        await TestTool.AddIdentityUser(_friend1, "friend-user1", testHost, context);
        await TestTool.AddIdentityUser(_friend2, "friend-user2", testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);
        var ticketGroup = await CreateGroup(testHost, context);
        var proposalRecord = await AddProposal(testHost, context);

        await RejectProposal(ticketGroup.TicketGroupId, proposalRecord.ProposalId, _principalId, testHost, context);
    }

    private async Task<TicketGroupRecord> CreateGroup(TicketShareTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var ticketGroup = new TicketGroupRecord
        {
            TicketGroupId = _ticketGroupId,
            Name = "Ticket Group Name",
            Description = "Sam's 2020 hockey tickets",

            Roles = _users,
            Seats = _seats,
        };

        var result = await client.Add(ticketGroup, context);
        result.IsOk().Should().BeTrue(result.ToString());

        return ticketGroup;
    }

    private async Task<ProposalRecord> AddProposal(TicketShareTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var proposal = new ProposalRecord
        {
            SeatId = "sec1-row1-seat1",
            SeatDate = new DateOnly(2024, 1, 1),
            Proposed = new StateDetail
            {
                Date = DateTime.UtcNow,
                ByPrincipalId = _principalId,
            }
        };

        var writeOption = await client.Proposal.Add(_ticketGroupId, proposal, context);
        writeOption.IsOk().Should().BeTrue(writeOption.ToString());
        return proposal;
    }

    private async Task AcceptProposal(string ticketGroupId, string proposalId, string principalId, TicketShareTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var acceptOption = await client.Proposal.Accept(ticketGroupId, proposalId, principalId, context);
        acceptOption.IsOk().Should().BeTrue(acceptOption.ToString());

        var ticketGroupOption = await client.Get(ticketGroupId, context);
        ticketGroupOption.IsOk().Should().BeTrue(ticketGroupOption.ToString());

        var ticketGroup = ticketGroupOption.Return();
        ticketGroup.ChangeLogs.Count.Should().Be(1);
        ticketGroup.Proposals.TryGetValue(proposalId, out var proposalRecord).Should().BeTrue();
        proposalRecord.NotNull().Accepted.Should().NotBeNull();
        proposalRecord.Rejected.Should().BeNull();
    }

    private async Task RejectProposal(string ticketGroupId, string proposalId, string principalId, TicketShareTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var acceptOption = await client.Proposal.Reject(ticketGroupId, proposalId, principalId, context);
        acceptOption.IsOk().Should().BeTrue(acceptOption.ToString());

        var ticketGroupOption = await client.Get(ticketGroupId, context);
        ticketGroupOption.IsOk().Should().BeTrue(ticketGroupOption.ToString());

        var ticketGroup = ticketGroupOption.Return();
        ticketGroup.ChangeLogs.Count.Should().Be(1);
        ticketGroup.Proposals.TryGetValue(proposalId, out var proposalRecord).Should().BeTrue();
        proposalRecord.NotNull().Accepted.Should().BeNull();
        proposalRecord.Rejected.Should().NotBeNull();
    }
}
