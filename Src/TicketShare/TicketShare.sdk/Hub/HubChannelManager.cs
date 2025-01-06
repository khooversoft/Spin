using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class HubChannelManager
{
    private readonly HubChannelClient _hubChannelClient;
    private readonly ILogger<HubChannelManager> _logger;

    public HubChannelManager(HubChannelClient hubChannelClient, ILogger<HubChannelManager> logger)
    {
        _hubChannelClient = hubChannelClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> CreateChannel(string channelId, string name, ScopeContext context)
    {
        channelId.NotEmpty();
        name.NotEmpty();

        var record = new HubChannelRecord
        {
            ChannelId = channelId,
            Name = name,
        };

        var result = await _hubChannelClient.Add(record, context);
        return result;
    }

    public async Task<Option<IReadOnlyList<ChannelInfo>>> GetChannelsInfo(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        var result = await _hubChannelClient.GetByPrincipalId(principalId, context);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<ChannelInfo>>();

        var channelInfos = result.Return().Select(x => new ChannelInfo
        {
            ChannelId = x.ChannelId,
            Name = x.Name,
            UnReadCount = x.GetUnreadMessageCount(principalId),
        }).ToImmutableArray();

        return channelInfos;
    }
}
