using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Identity.test;

public record TestContext
{
    public GraphTestClient Engine { get; init; } = null!;
    public IGraphClient GraphClient { get; init; } = null!;
    public IdentityClient IdentityClient { get; init; } = null!;
    public GraphMap Map { get; init; } = null!;
    public ScopeContext Context { get; init; }
}

internal static class TestTool
{
    public static TestContext CreateGraphEngineHost(ITestOutputHelper outputHelper)
    {
        GraphTestClient engine = GraphTestStartup.CreateGraphTestHost(config: x =>
        {
            x.AddToolboxIdentity();
            x.AddLogging(y => y.AddLambda(outputHelper.WriteLine));
        });

        var context = engine.GetScopeContext<PrincipalIdentityStoreTests>();

        IGraphClient graphClient = engine.ServiceProvider.GetRequiredService<IGraphClient>();
        IdentityClient identityClient = engine.ServiceProvider.GetRequiredService<IdentityClient>();
        GraphMap map = engine.ServiceProvider.GetRequiredService<IGraphHost>().Map;

        var result = new TestContext
        {
            Engine = engine,
            GraphClient = graphClient,
            IdentityClient = identityClient,
            Map = map,
            Context = context,
        };

        return result;
    }

    public static async Task CreateAndVerify(PrincipalIdentity user, TestContext testContext)
    {
        user.Validate().IsOk().Should().BeTrue();
        testContext.NotNull();

        var result = await testContext.IdentityClient.Set(user, testContext.Context);
        result.IsOk().Should().BeTrue();

        var readPrincipalIdentityOption = await testContext.IdentityClient.GetByPrincipalId(user.PrincipalId, testContext.Context);
        readPrincipalIdentityOption.IsOk().Should().BeTrue(readPrincipalIdentityOption.ToString());
        (user == readPrincipalIdentityOption.Return()).Should().BeTrue();

        var userNameOption = await testContext.IdentityClient.GetByName(user.UserName, testContext.Context);
        userNameOption.IsOk().Should().BeTrue();
        (user == userNameOption.Return()).Should().BeTrue();

        if (user.LoginProvider.IsNotEmpty() && user.ProviderKey.IsNotEmpty())
        {
            var readLoginOption = await testContext.IdentityClient.GetByLogin(user.LoginProvider, user.ProviderKey, testContext.Context);
            readLoginOption.IsOk().Should().BeTrue();
            (user == readLoginOption.Return()).Should().BeTrue();
        }
    }

    public static string UserId = "userName1@company.com";
    public static string UserEmail = "userName1@domain1.com";
    public static string UserName = "userName1";
    public static string LoginProvider = "loginProvider";
    public static string ProviderKey = "loginProvider.key1";

    public static PrincipalIdentity CreateUser()
    {
        var user = new PrincipalIdentity
        {
            PrincipalId = UserId,
            UserName = UserName,
            Email = UserEmail,
            NormalizedUserName = UserName.ToLower(),
            Name = "user name",
            LoginProvider = LoginProvider,
            ProviderKey = ProviderKey,
            ProviderDisplayName = "testProvider",
        };

        user.Validate().IsOk().Should().BeTrue();
        return user;
    }
}
