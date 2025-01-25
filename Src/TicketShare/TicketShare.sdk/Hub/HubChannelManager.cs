using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public partial class HubChannelManager
{
    protected readonly HubChannelClient _hubChannelClient;
    protected readonly ILogger<HubChannelManager> _logger;

    public HubChannelManager(HubChannelClient hubChannelClient, ILogger<HubChannelManager> logger)
    {
        _hubChannelClient = hubChannelClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> CreateChannel(string channelId, string name, string ownerPrincipalId, ScopeContext context)
    {
        channelId.NotEmpty();
        name.NotEmpty();
        ownerPrincipalId.NotEmpty();

        var record = new HubChannelRecord
        {
            ChannelId = channelId,
            Name = name,
            Users = new Dictionary<string, PrincipalRoleRecord>
            {
                [ownerPrincipalId] = new PrincipalRoleRecord
                {
                    PrincipalId = ownerPrincipalId,
                    Role = ChannelRole.Owner
                }
            }
        };

        var result = await _hubChannelClient.Add(record, context).ConfigureAwait(false);
        return result;
    }

    public async Task<Option<IReadOnlyList<ChannelInfo>>> GetChannelsInfo(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();
        var result = await _hubChannelClient.GetByPrincipalId(principalId, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<ChannelInfo>>();

        var channelInfos = result.Return().Select(x => new ChannelInfo
        {
            ChannelId = x.ChannelId,
            Name = x.Name,
            HasUnreadMessages = x.HasUnreadMessages(principalId)
        }).ToImmutableArray();

        return channelInfos;
    }

    public HubChannelContext GetContext(string channelId, string principalId, ScopeContext context)
    {
        channelId.NotEmpty();
        principalId.NotEmpty();

        return new HubChannelContext(channelId, principalId, _hubChannelClient, _logger);
    }
}
