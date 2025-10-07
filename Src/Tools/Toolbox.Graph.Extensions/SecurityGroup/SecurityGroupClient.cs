//using System.Collections.Immutable;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.Extensions;

//public class SecurityGroupClient
//{
//    private readonly IGraphClient _graphClient;
//    private readonly ILogger<SecurityGroupClient> _logger;

//    public SecurityGroupClient(IGraphClient graphClient, ILogger<SecurityGroupClient> logger)
//    {
//        _graphClient = graphClient.NotNull();
//        _logger = logger.NotNull();
//    }

//    public Task<Option> Create(string securityGroupId, string name, IEnumerable<(string user, SecurityAccess access)> access, ScopeContext context)
//    {
//        return Create(SecurityGroupTool.CreateRecord(securityGroupId, name, access), context);
//    }

//    public Task<Option> Create(string securityGroupId, string name, IEnumerable<PrincipalAccess> access, ScopeContext context)
//    {
//        return Create(SecurityGroupTool.CreateRecord(securityGroupId, name, access), context);
//    }

//    public async Task<Option> Create(SecurityGroupRecord securityGroupRecord, ScopeContext context)
//    {
//        context = context.With(_logger);
//        if (securityGroupRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(SecurityGroupRecord));

//        var cmdOption = securityGroupRecord.CreateQuery(false, context);
//        if (cmdOption.IsError()) return cmdOption.ToOptionStatus();

//        var cmd = cmdOption.Return();
//        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
//        result.LogStatus(context, "Set security group, nodeKey={nodeKey}", [securityGroupRecord.SecurityGroupId]);

//        return result.ToOptionStatus();
//    }

//    public SecurityGroupContext GetContext(string securityGroupId, string principalId) => new(_graphClient, securityGroupId, principalId, _logger);

//    public async Task<Option<IReadOnlyList<string>>> GroupsForPrincipalId(string principalId, ScopeContext context)
//    {
//        // SecurityGroup -> PrincipalId

//        var cmd = new SelectCommandBuilder()
//            .AddNodeSearch(x => x.SetNodeKey(IdentityTool.ToNodeKey(principalId)))
//            .AddRightJoin()
//            .AddEdgeSearch(x => x.SetEdgeType(SecurityGroupTool.EdgeType))
//            .AddRightJoin()
//            .AddNodeSearch(x => x.AddTag(SecurityGroupTool.NodeTag))
//            .Build();

//        var resultOption = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
//        resultOption.LogStatus(context, "Lookup security group by principalId={principalId}", [principalId]);
//        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<string>>();

//        var result = resultOption.Return();
//        if (result.Nodes.Count == 0) return (StatusCode.NotFound, "Node not found");

//        var list = result.Nodes.Select(x => SecurityGroupTool.RemoveNodeKeyPrefix(x.Key)).ToImmutableArray();
//        return list;
//    }

//    public async Task<Option> HasAccess(string securityGroupId, string principalId, SecurityAccess accessRequired, ScopeContext context)
//    {
//        var subject = await _graphClient.GetNode<SecurityGroupRecord>(SecurityGroupTool.ToNodeKey(securityGroupId), context).ConfigureAwait(false);
//        if (subject.IsError()) return subject.ToOptionStatus();

//        var read = subject.Return();
//        if (read.HasAccess(principalId, accessRequired).IsError(out var status))
//        {
//            return status.LogStatus(context, "HasAccess");
//        }

//        return StatusCode.OK;
//    }
//}
