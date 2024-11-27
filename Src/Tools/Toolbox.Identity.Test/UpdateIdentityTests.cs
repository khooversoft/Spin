using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Identity.test;

public class UpdateIdentityTests
{
    private ITestOutputHelper _outputHelper;
    public UpdateIdentityTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper.NotNull();

    [Fact]
    public async Task UpdateUserEmailInfo()
    {
        var engineContext = TestTool.CreateGraphEngineHost(_outputHelper);

        var userId = "userName1@company.com";
        var userEmail = "userName1@domain1.com";
        var userName = "userName1";
        var loginProvider = "loginProviderNet";
        var providerKey = "loginProvider.key1";

        var user = new PrincipalIdentity
        {
            PrincipalId = userId,
            UserName = userName,
            Email = userEmail,
            NormalizedUserName = userName.ToLower(),
            Name = "user name",
            LoginProvider = loginProvider,
            ProviderKey = providerKey,
            ProviderDisplayName = "testProvider",
        };

        user.Validate().IsOk().Should().BeTrue();

        await TestTool.CreateAndVerify(user, engineContext);
        engineContext.Map.Nodes.Count.Should().Be(1);
        engineContext.Map.Edges.Count.Should().Be(0);

        var list = new (string indexName, string key)[]
        {
            ("email", "username1@domain1.com"),
            ("loginProvider", "loginprovidernet/loginprovider.key1"),
            ("userName", "userName1"),
        };

        VerifyIndex(engineContext, list);

        var updatedUser = user with { Email = "newUserName@domainNew.com" };

        await TestTool.CreateAndVerify(updatedUser, engineContext);
        engineContext.Map.Nodes.Count.Should().Be(1);
        engineContext.Map.Edges.Count.Should().Be(0);

        list = new (string indexName, string key)[]
        {
            ("email", "newUserName@domainNew.com"),
            ("loginProvider", "loginprovidernet/loginprovider.key1"),
            ("userName", "userName1"),
        };

        VerifyIndex(engineContext, list);

        var deleteResult = await engineContext.IdentityClient.Delete(userId, engineContext.Context);
        deleteResult.IsOk().Should().BeTrue(deleteResult.ToString());
        engineContext.Map.Nodes.Count.Should().Be(0);
        engineContext.Map.Edges.Count.Should().Be(0);

        var selectCmd = new SelectCommandBuilder().AddNodeSearch(x => x.SetNodeKey(userId)).Build();

        var deleteOption = await engineContext.GraphClient.Execute(selectCmd, engineContext.Context);
        deleteOption.IsOk().Should().BeTrue();
        deleteOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
        });
    }

    private static void VerifyIndex(TestContext engineContext, (string indexName, string key)[] list)
    {
        foreach (var item in list)
        {
            var r = engineContext.Map.Nodes.LookupIndex(item.indexName, item.key);
            r.IsOk().Should().BeTrue(item.ToString());
            var rv = r.Return();
            rv.NodeKey.Should().Be("user:username1@company.com");
        }
    }
}
