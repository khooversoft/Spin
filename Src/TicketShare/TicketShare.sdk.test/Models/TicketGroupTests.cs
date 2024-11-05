using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Identity;
using Toolbox.Types;

namespace TicketShare.sdk.test.Account;

public class TicketGroupTests
{
    private const string _friendPrincipalId = "friend@domain.com";

    [Fact]
    public async Task FullLifeCycle()
    {
        var testHost = new TicketShareTestHost();
        var identityClient = testHost.ServiceProvider.GetRequiredService<IdentityClient>();
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<TicketGroupTests>();
        const string principalId = "user1@domain.com";

        var accountRecord = TestTool.Create(principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "user1", testHost, context);
        await TestTool.AddIdentityUser(_friendPrincipalId, "friend", testHost, context);
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
                .Append(new RoleRecord { PrincipalId = _friendPrincipalId, MemberRole = RoleType.Contributor })
                .ToArray(),
        };

        result = await client.Set(ticketGroup, context);
        result.IsOk().Should().BeTrue(result.ToString());

        readTicketGroup = await client.Get(ticketGroup.TicketGroupId, context);
        readTicketGroup.IsOk().Should().BeTrue();
        (ticketGroup == readTicketGroup.Return()).Should().BeTrue();

        (await client.GetByOwner(principalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == ticketGroup).Should().BeTrue();
            });
        });

        (await client.GetByMember(_friendPrincipalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == ticketGroup).Should().BeTrue();
            });
        });

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
                new SeatRecord { SeatId = "Sec-5-Row-7-Seat-8", AssignedToPrincipalId = principalId, Date = new DateTime(2024,1,10) },
                new SeatRecord { SeatId = "Sec-5-Row-7-Seat-9", AssignedToPrincipalId = principalId, Date = new DateTime(2024,1,10) },
                ],
        };

        var option = rec.Validate();
        option.IsOk().Should().BeTrue(option.ToString());

        return rec;
    }
}
