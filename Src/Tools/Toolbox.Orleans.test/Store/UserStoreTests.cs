using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Orleans.test.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Orleans.test.Store;

public class UserStoreTests : IClassFixture<ActorClusterFixture>
{
    private readonly ActorClusterFixture _actorCluster;

    public UserStoreTests(ActorClusterFixture actorCluster) => _actorCluster = actorCluster.NotNull();

    [Fact]
    public async Task AddLogon()
    {
        var userStore = _actorCluster.Cluster.ServiceProvider.GetRequiredService<UserStore>();

        var info = new UserLoginInfo("loginProvider", "providerKey", "providerDisplayName");
        string id = Guid.NewGuid().ToString();

        var userIdentity = new PrincipalIdentity
        {
            Id = id,
            UserName = "userName1",
            Email = "user@domain.com",
        };

        await userStore.AddLoginAsync(userIdentity, info, default);
        userIdentity.Id.Should().Be(id);
        userIdentity.UserName.Should().Be("userName1");
        userIdentity.Email.Should().Be("user@domain.com");
        userIdentity.LoginProvider.Should().Be("loginProvider");
        userIdentity.ProviderKey.Should().Be("providerKey");
        userIdentity.ProviderDisplayName.Should().Be("providerDisplayName");
    }

    [Fact]
    public async Task AddIdentity()
    {
        var userStore = _actorCluster.Cluster.ServiceProvider.GetRequiredService<UserStore>();
        var userIdentity = await TestUser(Guid.NewGuid().ToString(), "userName1", "userName1@domain.com", "providerKey1");

        IdentityResult deleteResult = await userStore.DeleteAsync(userIdentity, NullScopeContext.Instance);
        deleteResult.Succeeded.Should().BeTrue();

        (await userStore.FindByIdAsync(userIdentity.Id, NullScopeContext.Instance)).Should().BeNull();
        (await userStore.FindByLoginAsync("logonAuth", "providerKey1", default)).Should().BeNull();
        (await userStore.FindByNameAsync(userIdentity.UserName, NullScopeContext.Instance)).Should().BeNull();
    }

    [Fact]
    public async Task AddTwoIdentity()
    {
        var userStore = _actorCluster.Cluster.ServiceProvider.GetRequiredService<UserStore>();

        var userIdentity1 = await TestUser(Guid.NewGuid().ToString(), "userName1", "userName1@domain.com", "providerKey1");
        var userIdentity2 = await TestUser(Guid.NewGuid().ToString(), "userName2", "userName2@domain.com", "providerKey2");

        (await userStore.DeleteAsync(userIdentity1, NullScopeContext.Instance)).Action(async x =>
        {
            x.Succeeded.Should().BeTrue();

            (await userStore.FindByIdAsync(userIdentity1.Id, NullScopeContext.Instance)).Should().BeNull();
            (await userStore.FindByLoginAsync("logonAuth", "providerKey1", default)).Should().BeNull();
            (await userStore.FindByNameAsync(userIdentity1.UserName, NullScopeContext.Instance)).Should().BeNull();
        });

        (await userStore.DeleteAsync(userIdentity2, NullScopeContext.Instance)).Action(async x =>
        {
            x.Succeeded.Should().BeTrue();

            (await userStore.FindByIdAsync(userIdentity2.Id, NullScopeContext.Instance)).Should().BeNull();
            (await userStore.FindByLoginAsync("logonAuth", "providerKey2", default)).Should().BeNull();
            (await userStore.FindByNameAsync(userIdentity2.UserName, NullScopeContext.Instance)).Should().BeNull();
        });
    }

    private async Task<PrincipalIdentity> TestUser(string id, string userName, string userEmail, string providerKey)
    {
        var userStore = _actorCluster.Cluster.ServiceProvider.GetRequiredService<UserStore>();

        var userIdentity = new PrincipalIdentity
        {
            Id = id,
            UserName = userName,
            Email = userEmail,
            LoginProvider = "logonAuth",
            ProviderKey = providerKey,
            ProviderDisplayName = "helloProviderDisplay",
        };

        userIdentity.Validate().IsOk().Should().BeTrue();

        IdentityResult addResult = await userStore.CreateAsync(userIdentity, NullScopeContext.Instance);
        addResult.Succeeded.Should().BeTrue();

        PrincipalIdentity? idLookupIdentity = await userStore.FindByIdAsync(userIdentity.Id, NullScopeContext.Instance);
        idLookupIdentity.Should().NotBeNull();
        (userIdentity == idLookupIdentity).Should().BeTrue();

        PrincipalIdentity? logonLookupIdentity = await userStore.FindByLoginAsync("logonAuth", providerKey, default);
        logonLookupIdentity.Should().NotBeNull();
        (userIdentity == logonLookupIdentity).Should().BeTrue();

        PrincipalIdentity? userNameIdentity = await userStore.FindByNameAsync(userIdentity.UserName, NullScopeContext.Instance);
        userNameIdentity.Should().NotBeNull();
        (userIdentity == userNameIdentity).Should().BeTrue();

        return userIdentity;
    }
}
