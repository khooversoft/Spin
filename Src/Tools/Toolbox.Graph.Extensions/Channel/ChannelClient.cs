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

    public async Task<Option> Create(ChannelRecord channelRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (channelRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(channelRecord));

        var cmdOption = channelRecord.CreateQuery(false, context);
        if (cmdOption.IsError()) return cmdOption.ToOptionStatus();

        var cmd = cmdOption.Return();
        var result = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        result.LogStatus(context, "Set channel, channelId={channelId}", [channelRecord.ChannelId]);

        return result.ToOptionStatus();
    }

    public ChannelContext GetContext(string channelId, string principalId) => new(_graphClient, channelId, principalId, _logger);

    public async Task<Option<IReadOnlyList<ChannelRecord>>> GetPrincipalChannels(string principalId, ScopeContext context)
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
            .AddDataName("entity")
            .Build();

        var resultOption = await _graphClient.Execute(cmd, context).ConfigureAwait(false);
        resultOption.LogStatus(context, "Lookup security grup by principalId={principalId}", [principalId]);
        if (resultOption.IsError()) return resultOption.ToOptionStatus<IReadOnlyList<ChannelRecord>>();

        var result = resultOption.Return();
        IReadOnlyList<ChannelRecord> list = result.DataLinkToObjects<ChannelRecord>("entity");

        return list.ToOption();
    }

    public async Task<Option<IReadOnlyList<ChannelMessage>>> GetPrincipalMessages(string principalId, ScopeContext context)
    {
        var listOption = await GetPrincipalChannels(principalId, context);
        if (listOption.IsError()) return listOption.ToOptionStatus<IReadOnlyList<ChannelMessage>>();

        var list = listOption.Return()
            .SelectMany(x => x.Messages)
            .OrderBy(x => x.MessageId)
            .ToImmutableArray();

        return list;
    }
}
