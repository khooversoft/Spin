using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Identity.test;

public class UserStoreTests
{
    private ITestOutputHelper _outputHelper;
    public UserStoreTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper.NotNull();

    [Fact]
    public async Task UserStoreWithGraphInfo()
    {
        var engineContext = TestTool.CreateGraphEngineHost(_outputHelper);
        var userStore = engineContext.Engine.ServiceProvider.GetRequiredService<IUserStore<PrincipalIdentity>>();

        PrincipalIdentity user = TestTool.CreateUser(engineContext);

        var createResult = await userStore.CreateAsync(user, default);
        createResult.Succeeded.Should().BeTrue();
        engineContext.Map.Nodes.Count.Should().Be(4);
        engineContext.Map.Edges.Count.Should().Be(3);

        var userId = await userStore.GetUserIdAsync(user, default);
        userId.Should().Be(user.PrincipalId);

        var userName = await userStore.GetUserNameAsync(user, default);
        userName.Should().Be(user.UserName);

        var normalizeUserName = await userStore.GetNormalizedUserNameAsync(user, default);
        normalizeUserName.Should().Be(user.NormalizedUserName);

        string newNormalizeUserName = (TestTool.UserName + ".new").ToLower();
        await userStore.SetNormalizedUserNameAsync(user, newNormalizeUserName, default);
        var getNormalizeUserName = await userStore.GetNormalizedUserNameAsync(user, default);
        getNormalizeUserName.Should().Be(newNormalizeUserName);

        var updateResult = await userStore.UpdateAsync(user, default);
        updateResult.Succeeded.Should().BeTrue();

        PrincipalIdentity? findUser = await userStore.FindByIdAsync(user.PrincipalId, default);
        findUser.Should().NotBeNull();
        (user == findUser).Should().BeTrue();

        var findByName = await userStore.FindByNameAsync(newNormalizeUserName.NotEmpty(), default);
        (user == findByName).Should().BeTrue();

        var deleteResult = await userStore.DeleteAsync(user, default);
        deleteResult.Succeeded.Should().BeTrue();
        engineContext.Map.Nodes.Count.Should().Be(0);
        engineContext.Map.Edges.Count.Should().Be(0);
    }
}