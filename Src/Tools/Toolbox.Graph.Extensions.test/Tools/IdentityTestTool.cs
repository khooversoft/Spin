using Microsoft.Extensions.DependencyInjection;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.Tools;

internal class IdentityTestTool
{
    public static async Task AddIdentityUser(string principalId, string userName, GraphHostService testHost, ScopeContext context)
    {
        var client = testHost.Services.GetRequiredService<IdentityClient>();

        PrincipalIdentity user = new PrincipalIdentity
        {
            PrincipalId = principalId,
            Email = "em-" + principalId,
            UserName = userName,
            NormalizedUserName = userName.ToLowerInvariant(),
        };

        var result = await client.Set(user, context);
        result.IsOk().BeTrue(result.ToString());
    }
}
