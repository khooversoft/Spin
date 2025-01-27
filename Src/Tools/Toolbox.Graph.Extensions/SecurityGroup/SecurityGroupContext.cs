using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public class SecurityGroupContext
{
    private readonly string _securityGroupId;
    private readonly SecurityGroupClient _principalGroupClient;

    public SecurityGroupContext(string securityGroupId, SecurityGroupClient principalGroupClient)
    {
        _securityGroupId = securityGroupId.NotEmpty();
        _principalGroupClient = principalGroupClient.NotNull();
    }

    public async Task SetName(string name, ScopeContext context)
    {
        name.NotEmpty();
        var read = await _principalGroupClient.Get(_securityGroupId, context).ConfigureAwait(false);
        if (read.IsError()) read.LogStatus(context, "Get security group");

        var record = read.Value with
        {
            Name = name,
        };

        await _principalGroupClient.Set(record, context).ConfigureAwait(false);
        context.LogInformation("Updated name for group, securityGroupId={securityGroupId}, name={name}", _securityGroupId, name);
    }

    public async Task<Option<IReadOnlyList<MemberAccessRecord>>> GetAccess(ScopeContext context)
    {
        var read = await _principalGroupClient.Get(_securityGroupId, context).ConfigureAwait(false);
        if (read.IsError()) return read.LogStatus(context, "Get security group").ToOptionStatus<IReadOnlyList<MemberAccessRecord>>();

        return read.Return().Members.Values.ToImmutableArray();
    }

    public async Task<Option> SetAccess(string principalId, PrincipalAccess access, ScopeContext context)
    {
        principalId.NotEmpty();

        var read = await _principalGroupClient.Get(_securityGroupId, context).ConfigureAwait(false);
        if (read.IsError()) return read.LogStatus(context, "Get security group").ToOptionStatus();

        var accessRecord = new MemberAccessRecord { PrincipalId = principalId, Access = access };

        var record = read.Value with
        {
            Members = read.Value.Members.ToDictionary(StringComparer.OrdinalIgnoreCase)
                .Action(x => x[principalId] = accessRecord)
                .ToFrozenDictionary(),
        };

        await _principalGroupClient.Set(record, context).ConfigureAwait(false);
        context.LogInformation("Set member principalId={principalId} to group securityGroupId={securityGroupId}, access={access}", principalId, _securityGroupId, access);
        return StatusCode.OK;
    }

    public async Task<Option> RemoveAccess(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var read = await _principalGroupClient.Get(_securityGroupId, context).ConfigureAwait(false);
        if (read.IsError()) return read.LogStatus(context, "Get security group").ToOptionStatus();

        var record = read.Value with
        {
            Members = read.Value.Members.ToDictionary(StringComparer.OrdinalIgnoreCase)
                .Action(x => x.Remove(principalId))
                .ToFrozenDictionary(),
        };

        await _principalGroupClient.Set(record, context).ConfigureAwait(false);
        context.LogInformation("Remove member principalId={principalId} to group securityGroupId={securityGroupId}", principalId, _securityGroupId);
        return StatusCode.OK;
    }
}
