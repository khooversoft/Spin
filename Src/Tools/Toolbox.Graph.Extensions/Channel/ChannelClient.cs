using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public class ChannelClient
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<ChannelClient> _logger;

    public ChannelClient(IGraphClient graphClient, ILogger<ChannelClient> logger)
    {
        _graphClient = graphClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option> Add(ChannelRecord principalGroupRecord, ScopeContext context) => AddOrSet(false, principalGroupRecord, context);

    public async Task<Option> Delete(string channelId, ScopeContext context)
    {
        return await _graphClient.DeleteNode(ChannelTool.ToNodeKey(channelId), context).ConfigureAwait(false);
    }

    public async Task<Option<ChannelRecord>> Get(string channelId, ScopeContext context)
    {
        return await _graphClient.GetNode<ChannelRecord>(ChannelTool.ToNodeKey(channelId), context).ConfigureAwait(false);
    }

    public Task<Option> Set(ChannelRecord channelRecord, ScopeContext context) => AddOrSet(true, channelRecord, context);

    private async Task<Option> AddOrSet(bool useSet, ChannelRecord channelRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (channelRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(SecurityGroupRecord));

        string nodeKey = ChannelTool.ToNodeKey(channelRecord.ChannelId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(ChannelTool.NodeTag)
            .AddData("entity", channelRecord)
            .AddReference(ChannelTool.EdgeType, SecurityGroupTool.ToNodeKey(channelRecord.PrincipalGroupId))
            .Build();

        string cmds = new string[]
        {
            SecurityGroupRecord.Create(channelRecord.PrincipalGroupId, $"Auto principalGroup-{channelRecord.Name}").CreateQuery(true, context).Return(),
            cmd,
        }.Join(Environment.NewLine);

        var result = await _graphClient.Execute(cmds, context).ConfigureAwait(false);
        result.LogStatus(context, "Set channel, nodeKey={nodeKey}", [nodeKey]);
        if (result.IsError()) return result.ToOptionStatus();

        return StatusCode.OK;
    }

    public async Task<Option<IReadOnlyList<string>>> ChannelsForPrincipalId(string principalId, ScopeContext context)
    {
        // Channel -> PrincipalGroup -> PrincipalId

        var cmd = new SelectCommandBuilder()
            .AddNodeSearch(x => x.SetNodeKey(IdentityTool.ToNodeKey(principalId)))
            .AddRightJoin()
            .AddEdgeSearch(x => x.SetEdgeType(SecurityGroupTool.EdgeType))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(SecurityGroupTool.NodeTag))
            .AddRightJoin()
            .AddEdgeSearch(x => x.SetEdgeType(ChannelTool.EdgeType))
            .AddRightJoin()
            .AddNodeSearch(x => x.AddTag(ChannelTool.NodeTag))
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        resultOption.LogStatus(context, "Lookup security grup by principalId={principalId}", [principalId]);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<string>>();

        var result = resultOption.Return();
        if (result.Nodes.Count == 0) return (StatusCode.NotFound, "Node not found");

        var list = result.Nodes.Select(x => ChannelTool.RemoveNodeKeyPrefix(x.Key)).ToImmutableArray();
        return list;
    }
}
