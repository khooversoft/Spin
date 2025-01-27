using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public class SecurityGroupClient
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<SecurityGroupClient> _logger;

    public SecurityGroupClient(IGraphClient graphClient, ILogger<SecurityGroupClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> Add(SecurityGroupRecord securityGroupRecord, ScopeContext context) => AddOrSet(false, securityGroupRecord, context);

    public async Task<Option> Delete(string securityGroupId, ScopeContext context)
    {
        securityGroupId.NotEmpty();
        return await _graphClient.DeleteNode(SecurityGroupTool.ToNodeKey(securityGroupId), context).ConfigureAwait(false);
    }

    public async Task<Option<SecurityGroupRecord>> Get(string securityGroupId, ScopeContext context)
    {
        securityGroupId.NotEmpty();
        return await _graphClient.GetNode<SecurityGroupRecord>(SecurityGroupTool.ToNodeKey(securityGroupId), context).ConfigureAwait(false);
    }

    public async Task<Option> SetAccess(string securityGroupId, string principalId, PrincipalAccess access, ScopeContext context)
    {
        securityGroupId.NotEmpty();
        principalId.NotEmpty();

        var read = await Get(securityGroupId, context).ConfigureAwait(false);
        if (read.IsError()) return read.ToOptionStatus();
        var record = read.Return();

        var memberAccess = new MemberAccessRecord { PrincipalId = principalId, Access = access };
        var newRecord = record with
        {
            Members = record.Members.ToDictionary().Action(x => x[principalId] = memberAccess),
        };

        return await Set(newRecord, context).ConfigureAwait(false);
    }

    public async Task<Option> DeleteAccess(string securityGroupId, string principalId, ScopeContext context)
    {
        securityGroupId.NotEmpty();
        principalId.NotEmpty();

        var read = await Get(securityGroupId, context).ConfigureAwait(false);
        if (read.IsError()) return read.ToOptionStatus();
        var record = read.Return();

        if (!record.Members.ContainsKey(principalId)) return (StatusCode.NotFound, "PrincipalId not found");

        var newRecord = record with
        {
            Members = record.Members.ToDictionary().Action(x => x.Remove(principalId)),
        };

        return await Set(record, context).ConfigureAwait(false);
    }

    public Task<Option> Set(SecurityGroupRecord securityGroupRecord, ScopeContext context) => AddOrSet(true, securityGroupRecord, context);

    private async Task<Option> AddOrSet(bool useSet, SecurityGroupRecord securityGroupRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (securityGroupRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(SecurityGroupRecord));

        var cmdOption = securityGroupRecord.CreateQuery(useSet, context);
        if (cmdOption.IsError()) return cmdOption.ToOptionStatus();

        var cmd = cmdOption.Return();
        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        result.LogStatus(context, "Set security group, nodeKey={nodeKey}", [securityGroupRecord.SecurityGroupId]);

        return result.ToOptionStatus();
    }

    public async Task<Option<IReadOnlyList<string>>> GroupsForPrincipalId(string principalId, ScopeContext context)
    {
        // SecurityGroup -> PrincipalId

        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey(IdentityTool.ToNodeKey(principalId)))
            .AddRightJoin()
            .AddEdgeSearch(x => x.SetEdgeType(SecurityGroupTool.EdgeType))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(SecurityGroupTool.NodeTag))
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        resultOption.LogStatus(context, "Lookup security grup by principalId={principalId}", [principalId]);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<string>>();

        var result = resultOption.Return();
        if (result.Nodes.Count == 0) return (StatusCode.NotFound, "Node not found");

        var list = result.Nodes.Select(x => SecurityGroupTool.RemoveNodeKeyPrefix(x.Key)).ToImmutableArray();
        return list;
    }
}
