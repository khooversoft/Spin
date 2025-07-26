//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Tools;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.Extensions.test.Identity;

//public class UserStoreTests
//{
//    private ITestOutputHelper _outputHelper;
//    public UserStoreTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper.NotNull();

//    [Fact]
//    public async Task UserStoreWithGraphInfo()
//    {
//        using var graphTestClient = await GraphTestStartup.CreateGraphService(config: x => x.AddGraphExtensions());

//        var userStore = graphTestClient.Services.GetRequiredService<IUserStore<PrincipalIdentity>>();

//        PrincipalIdentity user = TestTool.CreateUser();

//        var createResult = await userStore.CreateAsync(user, default);
//        createResult.Succeeded.BeTrue();
//        graphTestClient.Map.Nodes.Count.Be(1);
//        graphTestClient.Map.Edges.Count.Be(0);

//        PrincipalIdentity findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
//        (user == findUser).BeTrue();

//        var userId = await userStore.GetUserIdAsync(user, default);
//        userId.Be(user.PrincipalId);

//        var userName = await userStore.GetUserNameAsync(user, default);
//        userName.Be(user.UserName);

//        var normalizeUserName = await userStore.GetNormalizedUserNameAsync(user, default);
//        normalizeUserName.Be(user.NormalizedUserName);

//        string newNormalizeUserName = (TestTool.UserName + ".new").ToLower();
//        await userStore.SetNormalizedUserNameAsync(user, newNormalizeUserName, default);
//        user.NormalizedUserName.Be(newNormalizeUserName);

//        var updateResult = await userStore.UpdateAsync(user, default);
//        updateResult.Succeeded.BeTrue();

//        findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
//        (user == findUser).BeTrue();

//        var getNormalizeUserName = await userStore.GetNormalizedUserNameAsync(user, default);
//        getNormalizeUserName.Be(newNormalizeUserName);

//        const string newUserName = "newUserName";
//        await userStore.SetUserNameAsync(user, newUserName, default);

//        updateResult = await userStore.UpdateAsync(user, default);
//        updateResult.Succeeded.BeTrue();

//        findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
//        (user == findUser).BeTrue();

//        const string providerDisplayName = "dkdk";
//        user = user with { ProviderDisplayName = providerDisplayName };

//        updateResult = await userStore.UpdateAsync(user, default);
//        updateResult.Succeeded.BeTrue();

//        findUser = (await userStore.FindByIdAsync(user.PrincipalId, default)).NotNull();
//        (user == findUser).BeTrue();

//        var findByName = await userStore.FindByNameAsync(newUserName.NotEmpty(), default);
//        (user == findByName).BeTrue();

//        var deleteResult = await userStore.DeleteAsync(user, default);
//        deleteResult.Succeeded.BeTrue();
//        graphTestClient.Map.Nodes.Count.Be(0);
//        graphTestClient.Map.Edges.Count.Be(0);
//    }
//}