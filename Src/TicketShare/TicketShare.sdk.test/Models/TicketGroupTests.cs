using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketShare.sdk.test.Account;

public class TicketGroupTests
{
    [Fact]
    public async Task FullLifeCycle()
    {
        var testHost = new TestHost();
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<TicketGroupTests>();
        const string principalId = "user1@domain.com";

        var accountRecord = TestTool.Create(principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);

        var ticketGroup = Create(principalId);
        var result = await client.Add(ticketGroup, context);
        result.IsOk().Should().BeTrue();

        var readTicketGroup = await client.Get(ticketGroup.TicketGroupId, context);
        readTicketGroup.IsOk().Should().BeTrue();

        (ticketGroup == readTicketGroup.Return()).Should().BeTrue();

        ticketGroup = ticketGroup with
        {
            Roles = ticketGroup.Roles
                .Append(new RoleRecord { PrincipalId = "friend@domain.com", MemberRole = RoleType.Contributor })
                .ToArray(),
        };

        result = await client.Set(ticketGroup, context);
        result.IsOk().Should().BeTrue();

        readTicketGroup = await client.Get(ticketGroup.TicketGroupId, context);
        readTicketGroup.IsOk().Should().BeTrue();
        (ticketGroup == readTicketGroup.Return()).Should().BeTrue();

        var delete = await client.Delete(ticketGroup.TicketGroupId, context);
        delete.IsOk().Should().BeTrue();

        readTicketGroup = await client.Get(ticketGroup.TicketGroupId, context);
        readTicketGroup.IsError().Should().BeTrue();
    }

    private TicketGroupRecord Create(string principalId)
    {
        var rec = new TicketGroupRecord
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

        var option = rec.Validate();
        option.IsOk().Should().BeTrue();

        return rec;
    }
}
