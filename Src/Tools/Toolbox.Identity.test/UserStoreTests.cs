using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Tools.Should;
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

        PrincipalIdentity user = TestTool.CreateUser();

        var createResult = await userStore.CreateAsync(user, default);
        createResult.Succeeded.Should().BeTrue();
        engineContext.Map.Nodes.Count.Should().Be(1);
        engineContext.Map.Edges.Count.Should().Be(0);

        PrincipalIdentity findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
        (user == findUser).Should().BeTrue();

        var userId = await userStore.GetUserIdAsync(user, default);
        userId.Should().Be(user.PrincipalId);

        var userName = await userStore.GetUserNameAsync(user, default);
        userName.Should().Be(user.UserName);

        var normalizeUserName = await userStore.GetNormalizedUserNameAsync(user, default);
        normalizeUserName.Should().Be(user.NormalizedUserName);

        string newNormalizeUserName = (TestTool.UserName + ".new").ToLower();
        await userStore.SetNormalizedUserNameAsync(user, newNormalizeUserName, default);
        user.NormalizedUserName.Should().Be(newNormalizeUserName);

        var updateResult = await userStore.UpdateAsync(user, default);
        updateResult.Succeeded.Should().BeTrue();

        findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
        (user == findUser).Should().BeTrue();

        var getNormalizeUserName = await userStore.GetNormalizedUserNameAsync(user, default);
        getNormalizeUserName.Should().Be(newNormalizeUserName);

        const string newUserName = "newUserName";
        await userStore.SetUserNameAsync(user, newUserName, default);

        updateResult = await userStore.UpdateAsync(user, default);
        updateResult.Succeeded.Should().BeTrue();

        findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
        (user == findUser).Should().BeTrue();

        const string providerDisplayName = "dkdk";
        user = user with { ProviderDisplayName = providerDisplayName };

        updateResult = await userStore.UpdateAsync(user, default);
        updateResult.Succeeded.Should().BeTrue();

        findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
        (user == findUser).Should().BeTrue();

        var findByName = await userStore.FindByNameAsync(newUserName.NotEmpty(), default);
        (user == findByName).Should().BeTrue();

        var deleteResult = await userStore.DeleteAsync(user, default);
        deleteResult.Succeeded.Should().BeTrue();
        engineContext.Map.Nodes.Count.Should().Be(0);
        engineContext.Map.Edges.Count.Should().Be(0);
    }
}