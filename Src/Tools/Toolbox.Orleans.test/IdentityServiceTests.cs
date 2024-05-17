using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Orleans.test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test;

public class IdentityServiceTests : IClassFixture<ClusterFixture>
{
    private readonly ClusterFixture _clusterFixture;
    public IdentityServiceTests(ClusterFixture clusterFixture) => _clusterFixture = clusterFixture.NotNull();

    [Fact]
    public async Task AddIdentity()
    {
        var service = _clusterFixture.Cluster.Client.GetIdentityActor();

        var userIdentity = new PrincipalIdentity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "userName1",
            Email = "userName1@domain.com",
            LoginProvider = "ms",
            ProviderKey = "providerKey",
        };

        userIdentity.Validate().IsOk().Should().BeTrue();

        (await service.Set(userIdentity, NullScopeContext.Instance)).IsOk().Should().BeTrue();

        (await service.GetById(userIdentity.Id, NullScopeContext.Instance)).Action(x =>
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

        (await service.Delete(userIdentity.Id, NullScopeContext.Instance)).IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task AddTwoIdentity()
    {
        var service = _clusterFixture.Cluster.Client.GetIdentityActor();

        const string userName1 = "userName1";
        const string userName2 = "userName2";

        await addIdentity(userName1, "userName1@domain.com", "providerkey1");
        await addIdentity(userName2, "userName2@domain.com", "providerkey2");


        (await service.GetByUserName(userName1, NullScopeContext.Instance)).Action(async x =>
        {
            x.IsOk().Should().BeTrue();
            var deleteResult = await service.Delete(x.Return().Id, NullScopeContext.Instance);
            deleteResult.IsOk().Should().BeTrue();
        });

        (await service.GetByUserName(userName2, NullScopeContext.Instance)).Action(async x =>
        {
            x.IsOk().Should().BeTrue();
            var deleteResult = await service.Delete(x.Return().Id, NullScopeContext.Instance);
            deleteResult.IsOk().Should().BeTrue();
        });

        async Task addIdentity(string userName, string email, string providerKey)
        {
            var userIdentity = new PrincipalIdentity
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                Email = email,
                LoginProvider = "ms",
                ProviderKey = providerKey,
            };

            userIdentity.Validate().IsOk().Should().BeTrue();

            (await service.Set(userIdentity, NullScopeContext.Instance)).IsOk().Should().BeTrue();

            (await service.GetById(userIdentity.Id, NullScopeContext.Instance)).Action(x =>
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
}