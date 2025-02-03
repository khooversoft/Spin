//using Microsoft.Extensions.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public class HubChannelContext
//{
//    private readonly HubChannelClient _hubChannelClient;
//    private readonly ILogger _logger;

//    public HubChannelContext(string channelId, string principalId, HubChannelClient hubChannelClient, ILogger logger)
//    {
//        ChannelId = channelId.NotEmpty();
//        PrincipalId = principalId.NotEmpty();
//        _hubChannelClient = hubChannelClient.NotNull();
//        _logger = logger.NotNull();

//        Principals = new HubChannelPrincipalAccess(this, _hubChannelClient);
//        Messages = new HubChannelMessageAccess(this, _hubChannelClient);
//    }

//    public string ChannelId { get; }
//    public string PrincipalId { get; }

//    public HubChannelPrincipalAccess Principals { get; }
//    public HubChannelMessageAccess Messages { get; }

//    public async Task<Option<HubChannelRecord>> Get(ScopeContext context)
//    {
//        var resultOption = await _hubChannelClient.Get(ChannelId, context);

//        var hubChannelRecord = resultOption.Return();
//        if (hubChannelRecord.HasAccess(PrincipalId, ChannelRole.Owner, context).IsError(out var r)) return r.ToOptionStatus<HubChannelRecord>();

//        return hubChannelRecord;
//    }
//}
