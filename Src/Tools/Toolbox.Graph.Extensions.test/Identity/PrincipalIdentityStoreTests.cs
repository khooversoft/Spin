using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.Extensions.test.Identity;

public class PrincipalIdentityStoreTests
{
    private ITestOutputHelper _outputHelper;

    public PrincipalIdentityStoreTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper.NotNull();

    [Fact]
    public async Task AddPrincipalId()
    {
        using var graphTestClient = await GraphTestStartup.CreateGraphService(config: x => x.AddGraphExtensions(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<PrincipalIdentityStoreTests>();
        var identityClient = graphTestClient.Services.GetRequiredService<IdentityClient>();

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
            NormalizedUserName = userName.ToLower(),
            Name = "user name",
            LoginProvider = loginProvider,
            ProviderKey = providerKey,
            ProviderDisplayName = "testProvider",
        };

        user.Validate().IsOk().Should().BeTrue();

        await TestTool.CreateAndVerify(user, graphTestClient, context);
        graphTestClient.Map.Nodes.Count.Should().Be(1);
        graphTestClient.Map.Nodes.First().Action(x =>
        {
            x.Key.Should().Be("user:username1@company.com");
            x.TagsString.Should().Be("email=username1@domain1.com,loginProvider=loginprovider/loginprovider.key1,principalIdentity,userName=username1");
        });

        graphTestClient.Map.Edges.Count.Should().Be(0);

        var deleteResult = await identityClient.Delete(userId, context);
        deleteResult.IsOk().Should().BeTrue();
        graphTestClient.Map.Nodes.Count.Should().Be(0);
        graphTestClient.Map.Edges.Count.Should().Be(0);

        var selectCmd = new SelectCommandBuilder().AddNodeSearch(x => x.SetNodeKey(userId)).Build();

        var deleteOption = await graphTestClient.Execute(selectCmd, context);
        deleteOption.IsOk().Should().BeTrue();
        deleteOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task AddMinimalPrincipalId()
    {
        using var graphTestClient = await GraphTestStartup.CreateGraphService(config: x => x.AddGraphExtensions());
        var context = graphTestClient.CreateScopeContext<PrincipalIdentityStoreTests>();
        var identityClient = graphTestClient.Services.GetRequiredService<IdentityClient>();

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

        await TestTool.CreateAndVerify(user, graphTestClient, context);
        graphTestClient.Map.Nodes.Count.Should().Be(1);
        graphTestClient.Map.Edges.Count.Should().Be(0);

        var deleteResult = await identityClient.Delete(userId, context);
        deleteResult.IsOk().Should().BeTrue();
        graphTestClient.Map.Nodes.Count.Should().Be(0);
        graphTestClient.Map.Edges.Count.Should().Be(0);

        var selectCmd = new SelectCommandBuilder().AddNodeSearch(x => x.SetNodeKey(userId)).Build();

        var deleteOption = await graphTestClient.Execute(selectCmd, context);
        deleteOption.IsOk().Should().BeTrue();
        deleteOption.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(0);
        });
    }
}