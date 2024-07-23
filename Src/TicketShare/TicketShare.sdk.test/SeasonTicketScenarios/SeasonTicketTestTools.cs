using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TicketShare.sdk.Actors;
using TicketShare.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Orleans;
using Toolbox.Types;

namespace TicketShare.sdk.test.SeasonTicketScenarios;

public static class SeasonTicketTestTools
{
    public static async Task<SeasonTicketRecord> CreateSeasonTicketRecord(ActorClusterFixture clusterFixture, string seasonTicketId, string ownerId)
    {
        var service = clusterFixture.Cluster.Client.GetSeasonTicketsActor();

        var seasonTicket = new SeasonTicketRecord
        {
            SeasonTicketId = seasonTicketId,
            Name = "Sea Hawks season 2024",
            Description = "Season 2024 tickets",
            OwnerPrincipalId = ownerId,
        };

        var result = await service.Set(seasonTicket, NullScopeContext.Instance);
        result.IsOk().Should().BeTrue(result.ToString());

        return seasonTicket;
    }

    public static async Task AddUser(ActorClusterFixture clusterFixture, string principalId)
    {
        var service = clusterFixture.Cluster.Client.GetIdentityActor();

        var userIdentity = new PrincipalIdentity
        {
            PrincipalId = principalId,
            UserName = $"{principalId}-userName",
            Email = $"{principalId}-userName@domain.com",
            LoginProvider = "ms",
            ProviderKey = $"{principalId}-providerKey",
        };

        userIdentity.Validate().IsOk().Should().BeTrue();
        await service.Delete(userIdentity.PrincipalId, NullScopeContext.Instance);

        (await service.Set(userIdentity, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        (await service.GetById(userIdentity.PrincipalId, NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            (userIdentity == x.Return()).Should().BeTrue();
        });

        (await service.GetByUserName(userIdentity.UserName, NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            (userIdentity == x.Return()).Should().BeTrue();
        });

        (await service.GetByEmail(userIdentity.Email, NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            (userIdentity == x.Return()).Should().BeTrue();
        });

        (await service.GetByLogin(userIdentity.LoginProvider, userIdentity.ProviderKey, NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            (userIdentity == x.Return()).Should().BeTrue();
        });
    }
}
