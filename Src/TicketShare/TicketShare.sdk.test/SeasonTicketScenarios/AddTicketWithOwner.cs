//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TicketShare.sdk.test.Application;
//using Toolbox.Tools;
//using TicketShare.sdk.Actors;
//using Toolbox.Orleans;
//using Toolbox.Types;
//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Identity;

//namespace TicketShare.sdk.test.SeasonTicketScenarios;

//public class AddTicketWithOwner : IClassFixture<ActorClusterFixture>
//{
//    private readonly ActorClusterFixture _actorFixture;
//    public AddTicketWithOwner(ActorClusterFixture clusterFixture) => _actorFixture = clusterFixture.NotNull();

//    [Fact]
//    public async Task AddSeasonTicket()
//    {
//        var service = _actorFixture.Cluster.Client.GetSeasonTicketsActor();

//        await AddUser("st-user1");
//        await AddUser("st-user2");
//        await AddUser("st-user3");

//        const string seasonTicketId = "user1/seahawks/2024";
//        var seasonTicketCreate = await AddTicket(seasonTicketId);

//        var seasonOption = await service.Get(seasonTicketId, NullScopeContext.Instance);
//        seasonOption.IsOk().Should().BeTrue();

//        var readSeasonTicket = seasonOption.Return();
//        (seasonTicketCreate == readSeasonTicket).Should().BeTrue();
//    }

//    private async Task AddUser(string principalId)
//    {
//        var service = _actorFixture.Cluster.Client.GetIdentityActor();

//        var userIdentity = new PrincipalIdentity
//        {
//            PrincipalId = principalId,
//            UserName = $"{principalId}-userName",
//            Email = $"{principalId}-userName@domain.com",
//            LoginProvider = "ms",
//            ProviderKey = $"{principalId}-providerKey",
//        };

//        userIdentity.Validate().IsOk().Should().BeTrue();
//        await service.Delete(userIdentity.PrincipalId, NullScopeContext.Instance);

//        (await service.Set(userIdentity, NullScopeContext.Instance)).IsOk().Should().BeTrue();

//        (await service.GetById(userIdentity.PrincipalId, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });

//        (await service.GetByUserName(userIdentity.UserName, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });

//        (await service.GetByEmail(userIdentity.Email, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });

//        (await service.GetByLogin(userIdentity.LoginProvider, userIdentity.ProviderKey, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });
//    }

//    private async Task<SeasonTicketRecord> AddTicket(string seasonTicketId)
//    {
//        var service = _actorFixture.Cluster.Client.GetSeasonTicketsActor();

//        var seasonTicket = new SeasonTicketRecord
//        {
//            SeasonTicketId = seasonTicketId,
//            Name = "Sea Hawks season 2024",
//            Description = "Season 2024 tickets",
//            OwnerPrincipalId = "st-user1",
//            Members = [
//                new RoleRecord { PrincipalId = "st-user2", MemberRole = RolePermission.Contributor },
//                ],
//            Properties = [
//                new Property { Key = "property1", Value = "value1" },
//                new Property { Key = "property2", Value = "value2" },
//                ],
//            Seats = [
//                new SeatRecord { SeatId = "R1S1", Date = DateTime.UtcNow, AssignedToPrincipalId = "st-User2" }
//                ],
//            ChangeLogs = [
//                    new ChangeLog
//                    {
//                        PropertyName = "Name",
//                        OldValue = null,
//                        NewValue = "Sea Hawks season 2024",
//                        ChangedByPrincipalId = "useradmin@domain.com",
//                        Date = DateTime.Now,
//                        Description = "hello"
//                    },
//                ],
//        };

//        var result = await service.Set(seasonTicket, NullScopeContext.Instance);
//        result.IsOk().Should().BeTrue(result.ToString());

//        return seasonTicket;
//    }
//}
