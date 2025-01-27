using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public class SecurityGroupManager
{
    private readonly SecurityGroupClient _client;
    public SecurityGroupManager(SecurityGroupClient principalGroupClient) => _client = principalGroupClient.NotNull();


    public async Task<Option> Create(string securityGroupId, string name, ScopeContext context)
    {
        securityGroupId.NotEmpty();
        name.NotEmpty();

        var record = new SecurityGroupRecord
        {
            SecurityGroupId = securityGroupId,
            Name = name,
            Members = FrozenDictionary<string, MemberAccessRecord>.Empty
        };

        var result = await _client.Add(record, context).ConfigureAwait(false);
        context.LogInformation("Created principal group, securityGroupId={securityGroupId}, name={name}", securityGroupId, name);
        return StatusCode.OK;
    }

    public SecurityGroupContext CreateContext(string securityGroupId) => new SecurityGroupContext(securityGroupId, _client);

    public Task<Option<IReadOnlyList<string>>> GroupsForPrincipalId(string principalId, ScopeContext context) => _client.GroupsForPrincipalId(principalId, context);
}
