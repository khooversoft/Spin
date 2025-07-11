using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.Identity;

internal static class TestTool
{
    public static async Task CreateAndVerify(PrincipalIdentity user, GraphHostService graphHostService, ScopeContext context)
    {
        user.Validate().IsOk().BeTrue();
        graphHostService.NotNull();

        var identityClient = graphHostService.Services.GetRequiredService<IdentityClient>();
        var result = await identityClient.Set(user, context);
        result.IsOk().BeTrue();

        var readPrincipalIdentityOption = await identityClient.GetByPrincipalId(user.PrincipalId, context);
        readPrincipalIdentityOption.IsOk().BeTrue();
        (user == readPrincipalIdentityOption.Return()).BeTrue();

        var userNameOption = await identityClient.GetByName(user.UserName, context);
        userNameOption.IsOk().BeTrue();
        (user == userNameOption.Return()).BeTrue();

        if (user.LoginProvider.IsNotEmpty() && user.ProviderKey.IsNotEmpty())
        {
            var readLoginOption = await identityClient.GetByLogin(user.LoginProvider, user.ProviderKey, context);
            readLoginOption.IsOk().BeTrue();
            (user == readLoginOption.Return()).BeTrue();
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

        user.Validate().IsOk().BeTrue();
        return user;
    }
}
