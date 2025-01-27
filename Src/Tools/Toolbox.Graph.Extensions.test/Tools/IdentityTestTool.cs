using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph.Extensions.Testing;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.test.Tools;

internal class IdentityTestTool
{
    public static async Task AddIdentityUser(string principalId, string userName, ToolboxExtensionTestHost testHost, ScopeContext context)
    {
        var client = testHost.ServiceProvider.GetRequiredService<IdentityClient>();

        PrincipalIdentity user = new PrincipalIdentity
        {
            PrincipalId = principalId,
            Email = "em-" + principalId,
            UserName = userName,
            NormalizedUserName = userName.ToLowerInvariant(),
        };

        var result = await client.Set(user, context);
        result.IsOk().Should().BeTrue(result.ToString());
    }
}
