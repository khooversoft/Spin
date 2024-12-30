using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class HubChannelMessageClient
{
    private readonly ILogger<HubChannelMessageClient> _logger;
    private readonly HubChannelClient _hubChannelClient;

    public HubChannelMessageClient(HubChannelClient hubChannelClient, ILogger<HubChannelMessageClient> logger)
    {
        _hubChannelClient = hubChannelClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Send(ChannelMessageRecord messageRecord, ScopeContext context)
    {
        context = context.With(_logger);
        if (messageRecord.Validate().TryError(out var r)) return r.LogStatus(context, nameof(ChannelMessageRecord));

        var readOption = await _hubChannelClient.Get(messageRecord.ChannelId, context);
        if (readOption.IsError()) return readOption.ToOptionStatus();
        HubChannelRecord read = readOption.Return();

        var newHubChannelRecord = read.ToBuilder().AddMessage(messageRecord).Build();

        var writeOption = await _hubChannelClient.Set(newHubChannelRecord, context);
        return writeOption;
    }

    public async Task<Option<IReadOnlyList<ChannelState>>> GetChannelStatesForPrincipalId(string principalId, ScopeContext context)
    {
        principalId.NotEmpty();

        var readOption = await _hubChannelClient.GetByPrincipalId(principalId, context);
        if (readOption.IsError()) return readOption.ToOptionStatus<IReadOnlyList<ChannelState>>();

        var read = readOption.Return();

        var list = read
            .SelectMany(x => x.GetMessages(principalId).Where(x => x.ReadDate == null))
            .GroupBy(x => x.Message.ChannelId)
            .Select(x => new ChannelState(x.Key, x.Count()))
            .ToArray();

        return list;
    }

    public async Task<Option> MarkRead(string channelId, string principalId, IEnumerable<string> messageIds, DateTime readDate, ScopeContext context)
    {
        channelId.NotEmpty();
        principalId.NotEmpty();
        messageIds.NotNull();

        var readOption = await _hubChannelClient.Get(channelId, context);
        if (readOption.IsError()) return readOption.ToOptionStatus();
        var read = readOption.Return();

        var updateModel = read.ToBuilder()
            .Action(x => messageIds.ForEach(y => x.MarkRead(principalId, y, readDate)))
            .Build();

        var result = await _hubChannelClient.Set(updateModel, context);
        return result;
    }
}
