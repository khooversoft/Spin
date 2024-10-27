using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Identity.test;

public class PrincipalIdentityStoreTests
{
    private ITestOutputHelper _outputHelper;

    public PrincipalIdentityStoreTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper.NotNull();

    [Fact]
    public async Task AddPrincipalId()
    {
        var engineContext = TestTool.CreateGraphEngineHost(_outputHelper);

        var userId = "userName1@company.com";
        var userEmail = "userName1@domain1.com";
        var userName = "userName1";
        var loginProvider = "loginProvider";
        var providerKey = "loginProvider.key1";

        var user = new PrincipalIdentity
        {
            PrincipalId = userId,
            UserName = userName,
            Email = userEmail,
            EmailConfirmed = true,
            PasswordHash = "passwordHash",
            NormalizedUserName = userName.ToLower(),
            AuthenticationType = "auth",
            IsAuthenticated = true,
            Name = "user name",
            LoginProvider = loginProvider,
            ProviderKey = providerKey,
            ProviderDisplayName = "testProvider",
        };

        user.Validate().IsOk().Should().BeTrue();

        await TestTool.CreateAndVerify(user, engineContext);
        engineContext.Map.Nodes.Count.Should().Be(1);
        engineContext.Map.Nodes.First().Action(x =>
        {
            x.Key.Should().Be("user:username1@company.com");
            x.TagsString.Should().Be("email=username1@domain1.com,loginProvider=loginprovider/loginprovider.key1,userName=username1");
        });

        engineContext.Map.Edges.Count.Should().Be(0);

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

    [Fact]
    public async Task AddMinimalPrincaipalId()
    {
        var engineContext = TestTool.CreateGraphEngineHost(_outputHelper);

        var userId = "userName1@company.com";
        var userEmail = "userName1@domain1.com";
        var userName = "userName1";
        var normalizeUserName = userName.ToLower();

        var user = new PrincipalIdentity
        {
            PrincipalId = userId,
            UserName = userName,
            Email = userEmail,
            NormalizedUserName = userName.ToLower(),
        };

        user.Validate().IsOk().Should().BeTrue();

        await TestTool.CreateAndVerify(user, engineContext);
        engineContext.Map.Nodes.Count.Should().Be(1);
        engineContext.Map.Edges.Count.Should().Be(0);

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
}