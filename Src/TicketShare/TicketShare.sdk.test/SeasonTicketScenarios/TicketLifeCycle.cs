using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TicketShare.sdk.Actors;
using TicketShare.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.test.SeasonTicketScenarios;

public class TicketLifeCycle : IClassFixture<ActorClusterFixture>
{
    private readonly ActorClusterFixture _actorFixture;
    public TicketLifeCycle(ActorClusterFixture clusterFixture) => _actorFixture = clusterFixture.NotNull();

    [Fact]
    public async Task BuildTicketAndAssignScheduleAndSeats()
    {
        var service = _actorFixture.Cluster.Client.GetSeasonTicketsActor();
        var directory = _actorFixture.Cluster.Client.GetDirectoryActor();

        await SeasonTicketTestTools.AddUser(_actorFixture, "s1-user1");
        await SeasonTicketTestTools.AddUser(_actorFixture, "s1-user2");
        await SeasonTicketTestTools.AddUser(_actorFixture, "s1-user3");

        const string seasonTicketId = "user1/seahawks/2024";
        var seasonTicketCreate = await SeasonTicketTestTools.CreateSeasonTicketRecord(_actorFixture, seasonTicketId, "s1-user1");

        RoleRecord addRole2 = new RoleRecord { PrincipalId = "s1-user2", MemberRole = RolePermission.Contributor };
        (await service.AddRole(seasonTicketId, addRole2, NullScopeContext.Instance)).ThrowOnError();

        RoleRecord addRole3 = new RoleRecord { PrincipalId = "s1-user3", MemberRole = RolePermission.ReadOnly };
        (await service.AddRole(seasonTicketId, addRole3, NullScopeContext.Instance)).ThrowOnError();

        Property property1 = new Property { Key = "pkey1", Value = "pkey1-value" };
        (await service.AddProperty(seasonTicketId, property1, NullScopeContext.Instance)).ThrowOnError();

        Property property2 = new Property { Key = "pkey2", Value = "pkey2-value" };
        (await service.AddProperty(seasonTicketId, property2, NullScopeContext.Instance)).ThrowOnError();

        var seatList = await CreateSeats(seasonTicketId, new[] { "s1-seat1", "s1-seat2", "s1-seat3" }, DateTime.Now, 10);

        var readSeasonTicket = (await service.Get(seasonTicketId, NullScopeContext.Instance)).ThrowOnError().Return();

        readSeasonTicket.SeasonTicketId.Should().Be(seasonTicketId);
        readSeasonTicket.Name.Should().Be(seasonTicketCreate.Name);
        readSeasonTicket.Description.Should().Be(seasonTicketCreate.Description);
        readSeasonTicket.OwnerPrincipalId.Should().Be(seasonTicketCreate.OwnerPrincipalId);
        readSeasonTicket.Tags.Should().Be(seasonTicketCreate.Tags);

        readSeasonTicket.Properties.NotNull().Count.Should().Be(2);
        Enumerable.SequenceEqual(readSeasonTicket.Properties.OrderBy(x => x.Key), new[] { property1, property2 }).Should().BeTrue();

        readSeasonTicket.Members.NotNull().Count.Should().Be(2);
        Enumerable.SequenceEqual(readSeasonTicket.Members.OrderBy(x => x.PrincipalId), new[] { addRole2, addRole3 }).Should().BeTrue();

        readSeasonTicket.Seats.NotNull().Count.Should().Be(30);
        Enumerable.SequenceEqual(readSeasonTicket.Seats, seatList).Should().BeTrue();

        // Lookup season ticket based on user
        foreach (var userId in new[] { "s1-user1", "s1-user2", "s1-user3" })
        {
            var s1 = await directory.Execute(SeasonTicketRecord.GetSeasonTicketsForUser(userId), NullScopeContext.Instance);
            s1.IsOk().Should().BeTrue(userId);
            s1.Return().Action(x =>
            {
                var nodes = x.Items.OfType<GraphNode>().ToArray();
                nodes.Length.Should().Be(1, userId);
                nodes.First().Key.Should().Be(TicketShareTool.ToSeasonTicketKey(seasonTicketId), userId);
            });
        }
    }

    private async Task<IReadOnlyList<SeatRecord>> CreateSeats(string seasonTicketId, string[] seats, DateTime startDate, int numberOfWeeks)
    {
        var service = _actorFixture.Cluster.Client.GetSeasonTicketsActor();

        var seatRecords = Enumerable.Range(0, numberOfWeeks)
            .SelectMany(_ => seats, (o, i) => new SeatRecord { SeatId = i, Date = startDate.AddDays(o * 7) })
            .ToArray();

        foreach (var seat in seatRecords)
        {
            (await service.AddSeat(seasonTicketId, seat, NullScopeContext.Instance)).ThrowOnError();
        }

        return seatRecords;
    }
}
