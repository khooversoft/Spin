using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace TicketShare.sdk.test.TicketGroup;

public class TicketGroupManagerTests
{
    private const string _roleId2 = "roleId2";
    private const string _modelName = "sam/2020/hockey";
    private const string _modelDescription = "Sam's 2020 hockey tickets";
    private const string _modelChannelId = "user1@domain.com/sam/2020/hockey/channelHub";
    private const string _modelSeat1Id = "seat1-id";
    private const string _modelSeat1 = "Sec-5-Row-7-Seat-8";
    private const string _modelSeat2Id = "seat2-id";
    private const string _modelSeat2 = "Sec-5-Row-7-Seat-9";
    private static readonly DateTime _seatDate = new DateTime(2024, 1, 10);

    [Fact]
    public async Task FullLifeCycle()
    {
        const string principalId = "user1@domain.com";
        const string friendPrincipalId = "friend@domain.com";

        var testHost = await GraphTestStartup.CreateGraphService();
        var identityClient = testHost.Services.GetRequiredService<IdentityClient>();
        var manager = testHost.Services.GetRequiredService<TicketGroupManager>();
        var context = testHost.CreateScopeContext<TicketGroupTests>();

        var accountRecord = TestTool.CreateAccountModel(principalId);
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "user1", testHost, context);
        await TestTool.AddIdentityUser(friendPrincipalId, "friend", testHost, context);
        await TestTool.AddAccount(accountRecord, testHost, context);

        var createAccount = await manager.Create(new TicketGroupHeaderModel { Name = _modelName, Description = _modelDescription }, context);
        createAccount.IsOk().Should().BeTrue();

        var ticketContext = manager.GetContext(createAccount.Return());
        (await ticketContext.Get(context)).IsOk().Should().BeTrue();
        var principalModelId = ticketContext.Input.Roles.Single().Value.Id;

        SeatModel s1 = new SeatModel { Id = _modelSeat1Id, Section = "1", Row = "10", Seat = _modelSeat1, AssignedToPrincipalId = principalId, Date = _seatDate };
        (await ticketContext.Seats.Set(s1, context)).IsOk().Should().BeTrue();

        SeatModel s2 = new SeatModel { Id = _modelSeat2Id, Section = "1", Row = "10", Seat = _modelSeat2, AssignedToPrincipalId = principalId, Date = _seatDate };
        (await ticketContext.Seats.Set(s2, context)).IsOk().Should().BeTrue();

        (await ticketContext.Get(context)).IsOk().Should().BeTrue();

        var expectedModel = CreateTicketGroupModel(createAccount.Return(), principalId, principalModelId, null).ConvertTo();
        (ticketContext.Input == expectedModel).Should().BeTrue();

        RoleModel s3 = new RoleModel { Id = _roleId2, PrincipalId = friendPrincipalId, MemberRole = RoleType.Contributor.ToString() };
        (await ticketContext.Roles.Set(s3, context)).IsOk().Should().BeTrue();
        (await ticketContext.Get(context)).IsOk().Should().BeTrue();

        expectedModel = CreateTicketGroupModel(createAccount.Return(), principalId, principalModelId, friendPrincipalId).ConvertTo();
        (ticketContext.Input == expectedModel).Should().BeTrue();

        var expectedRecord = expectedModel.ConvertTo();

        (await manager.Search(principalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == expectedRecord).Should().BeTrue();
            });
        });

        (await manager.Search(friendPrincipalId, context)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Action(y =>
            {
                y.Count.Should().Be(1);
                (y[0] == expectedRecord).Should().BeTrue();
            });
        });

        var delete = await ticketContext.Delete(context);
        delete.IsOk().Should().BeTrue();

        (await ticketContext.Get(context)).IsOk().Should().BeFalse();
    }

    private TicketGroupRecord CreateTicketGroupModel(string ticketGroupId, string principalId, string principalModelId, string? contributorPrincipalId)
    {
        var ticketGroup = new TicketGroupRecord
        {
            TicketGroupId = ticketGroupId,
            Name = _modelName,
            Description = _modelDescription,
            ChannelId = _modelChannelId,

            Roles = [
                new RoleRecord { Id = principalModelId, PrincipalId = principalId, MemberRole = RoleType.Owner },
                ],

            Seats = [
                new SeatRecord { Id = _modelSeat1Id, Section = "1", Row = "10", Seat = _modelSeat1, AssignedToPrincipalId = principalId, Date = new DateTime(2024,1,10) },
                new SeatRecord { Id = _modelSeat2Id, Section = "1", Row = "10", Seat = _modelSeat2, AssignedToPrincipalId = principalId, Date = new DateTime(2024,1,10) },
                ],
        };

        if (contributorPrincipalId != null)
        {
            ticketGroup = ticketGroup with
            {
                Roles = ticketGroup.Roles
                    .Append(new RoleRecord { Id = _roleId2, PrincipalId = contributorPrincipalId, MemberRole = RoleType.Contributor })
                    .ToArray(),
            };
        }

        return ticketGroup;
    }
}
