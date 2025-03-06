using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Graph.Extensions;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace TicketShare.sdk.test.TicketGroup;

public class TicketGroupTests
{
    [Fact]
    public async Task FullLifeCycle()
    {
        var testHost = new TicketShareTestHost();
        var identityClient = testHost.ServiceProvider.GetRequiredService<IdentityClient>();
        var ticketGroupClient = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<TicketGroupTests>();
        const string principalId = "user1@domain.com";
        const string friendPrincipalId = "friend@domain.com";

        var accountRecord = TestTool.CreateAccountModel(principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "user1", testHost, context);
        await TestTool.AddIdentityUser(friendPrincipalId, "friend", testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);

        var ticketGroup = CreateTicketGroupModel("sam/2020/hockey", principalId, null);
        var result = await ticketGroupClient.Add(ticketGroup, context);
        result.IsOk().Should().BeTrue(result.ToString());

        var readTicketGroupOption = await ticketGroupClient.Get(ticketGroup.TicketGroupId, context);
        readTicketGroupOption.IsOk().Should().BeTrue();
        var readTicketGroup = readTicketGroupOption.Return();

        ticketGroup = ticketGroup with { ChannelId = readTicketGroup.ChannelId };
        (ticketGroup == readTicketGroup).Should().BeTrue();

        ticketGroup = ticketGroup with
        {
            Roles = ticketGroup.Roles
                .Append(new RoleRecord { PrincipalId = friendPrincipalId, MemberRole = RoleType.Contributor })
                .ToArray(),
        };

        result = await ticketGroupClient.Set(ticketGroup, context);
        result.IsOk().Should().BeTrue(result.ToString());

        readTicketGroupOption = await ticketGroupClient.Get(ticketGroup.TicketGroupId, context);
        readTicketGroupOption.IsOk().Should().BeTrue();

        (await ticketGroupClient.Search(principalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == ticketGroup).Should().BeTrue();
            });
        });

        (await ticketGroupClient.Search(friendPrincipalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == ticketGroup).Should().BeTrue();
            });
        });

        var delete = await ticketGroupClient.Delete(ticketGroup.TicketGroupId, context);
        delete.IsOk().Should().BeTrue();

        readTicketGroupOption = await ticketGroupClient.Get(ticketGroup.TicketGroupId, context);
        readTicketGroupOption.IsError().Should().BeTrue();
    }

    [Fact]
    public async Task TwoTicketGroupsFullLifeCycle()
    {
        var testHost = new TicketShareTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();
        var context = testHost.GetScopeContext<TicketGroupTests>();
        const string principalIdOne = "user1@domain.com";
        const string ticketGroupIdOne = "sam/2020/hockey1";
        const string friend1 = "friend1@domain.com";
        const string friend2 = "friend2@domain.com";
        const string principalIdTwo = "user2@domain.com";
        const string ticketGroupIdTwo = "sam/2020/hockey2";

        await CreateAccountAndTicketGroup(ticketGroupIdOne, principalIdOne, friend1, testHost, context);
        await CreateAccountAndTicketGroup(ticketGroupIdTwo, principalIdTwo, friend2, testHost, context);

        await getAndTest(ticketGroupIdOne, async () => await client.Search(principalIdOne, context));
        await getAndTest(ticketGroupIdOne, async () => await client.Search(friend1, context));

        await getAndTest(ticketGroupIdTwo, async () => await client.Search(principalIdTwo, context));
        await getAndTest(ticketGroupIdTwo, async () => await client.Search(friend2, context));

        async Task<IReadOnlyList<TicketGroupRecord>> getAndTest(string ticketGroupId, Func<Task<Option<IReadOnlyList<TicketGroupRecord>>>> getFunc)
        {
            var result = await getFunc();
            result.IsOk().Should().BeTrue();
            var subject = result.Return();
            subject.Count.Should().Be(1);
            subject[0].TicketGroupId.Should().Be(ticketGroupId);

            return subject;
        }
    }

    private async Task CreateAccountAndTicketGroup(string ticketGroupId, string principalId, string friendPrincipalId, TicketShareTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<TicketGroupClient>();

        var accountRecord = TestTool.CreateAccountModel(principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "user1" + principalId, testHost, context);
        await TestTool.AddIdentityUser(friendPrincipalId, "friend" + friendPrincipalId, testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);

        var ticketGroup = CreateTicketGroupModel(ticketGroupId, principalId, friendPrincipalId);
        (await client.Add(ticketGroup, context)).IsOk().Should().BeTrue();

        var readTicketGroupOption = await client.Get(ticketGroup.TicketGroupId, context);
        readTicketGroupOption.IsOk().Should().BeTrue();

        var readTicketGroup = readTicketGroupOption.Return();
        ticketGroup = ticketGroup with { ChannelId = readTicketGroup.ChannelId };
        (ticketGroup == readTicketGroup).Should().BeTrue();

        readTicketGroupOption = await client.Get(ticketGroup.TicketGroupId, context);
        readTicketGroupOption.IsOk().Should().BeTrue();
        (ticketGroup == readTicketGroupOption.Return()).Should().BeTrue();

        (await client.Search(principalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == ticketGroup).Should().BeTrue();
            });
        });

        (await client.Search(friendPrincipalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == ticketGroup).Should().BeTrue();
            });
        });
    }

    private TicketGroupRecord CreateTicketGroupModel(string ticketGroupId, string principalId, string? contributorPrincipalId)
    {
        var ticketGroup = new TicketGroupRecord
        {
            TicketGroupId = ticketGroupId,
            Name = "name",
            Description = "Sam's 2020 hockey tickets",

            Roles = [
                new RoleRecord { PrincipalId = principalId, MemberRole = RoleType.Owner },
                ],

            Seats = [
                new SeatRecord { Section = "1", Row = "10", Seat = "Sec-5-Row-7-Seat-8", AssignedToPrincipalId = principalId, Date = new DateTime(2024,1,10) },
                new SeatRecord { Section = "1", Row = "10", Seat = "Sec-5-Row-7-Seat-9", AssignedToPrincipalId = principalId, Date = new DateTime(2024,1,10) },
                ],
        };

        if (contributorPrincipalId != null)
        {
            ticketGroup = ticketGroup with
            {
                Roles = ticketGroup.Roles
                    .Append(new RoleRecord { PrincipalId = contributorPrincipalId, MemberRole = RoleType.Contributor })
                    .ToArray(),
            };
        }

        return ticketGroup;
    }
}
