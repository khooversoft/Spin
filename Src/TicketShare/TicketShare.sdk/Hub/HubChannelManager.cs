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

        Principal = new HubChannelPrincipalActor(hubChannelClient);
        Messages = new HubChannelMessageActor(hubChannelClient);
    }

    public HubChannelPrincipalActor Principal { get; }
    public HubChannelMessageActor Messages { get; }

    public async Task<Option> CreateChannel(string channelId, string name, string ownerPrincipalId, ScopeContext context)
    {
        channelId.NotEmpty();
        name.NotEmpty();
        ownerPrincipalId.NotEmpty();

        var record = new HubChannelRecord
        {
            ChannelId = channelId,
            Name = name,
            Users = new Dictionary<string, PrincipalChannelRecord>
            {
                [ownerPrincipalId] = new PrincipalChannelRecord
                {
                    PrincipalId = ownerPrincipalId,
                    Role = ChannelRole.Owner
                }
            }
        };

        var result = await _hubChannelClient.Add(record, context);
        return result;
    }

    public async Task<Option<HubChannelRecord>> Get(string principalId, string channelId, ScopeContext context)
    {
        var resultOption = await _hubChannelClient.Get(channelId, context);

        var hubChannelRecord = resultOption.Return();
        if (hubChannelRecord.HasAccess(principalId, ChannelRole.Owner, context).IsError(out var r)) return r.ToOptionStatus<HubChannelRecord>();

        return hubChannelRecord;
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
            HasUnreadMessages = x.HasUnreadMessages(principalId)
        }).ToImmutableArray();

        return channelInfos;
    }

    public async Task<Option<IReadOnlyList<HubChannelRecord>>> GetByPrincipalId(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var result = await _hubChannelClient.GetByPrincipalId(principalId, context);
        return result;
    }

    //public async Task<Option> HasAccess(string channelId, string principalId, RoleType requiredAccess, ScopeContext context)
    //{
    //    var result = await Principal.GetRole(channelId, principalId, context);
    //    if (result.IsError()) return result.ToOptionStatus();

    //    ChannelRole role = result.Return();
    //    var hasAccess = requiredAccess switch
    //    {
    //        RoleType.ReadOnly => true,
    //        RoleType.Contributor => role == ChannelRole.Contributor || role == ChannelRole.Owner,
    //        RoleType.Owner => role == ChannelRole.Owner,

    //        _ => throw new ArgumentException($"Unknown required access: {requiredAccess}")
    //    };

    //    return hasAccess ? StatusCode.OK : StatusCode.Forbidden;
    //}
}
