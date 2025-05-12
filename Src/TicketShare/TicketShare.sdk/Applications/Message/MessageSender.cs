using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class MessageSender
{
    private readonly Channel<ChannelMessage> _channel;
    private readonly ILogger<MessageSender> _logger;

    public MessageSender(Channel<ChannelMessage> channel, ILogger<MessageSender> logger)
    {
        _channel = channel.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Send(ChannelMessage channelMessage, ScopeContext context)
    {
        context = context.With(_logger);
        if (channelMessage.Validate().IsError(out var r)) return r.LogStatus(context, nameof(ChannelMessage));

        await _channel.Writer.WriteAsync(channelMessage).ConfigureAwait(false);
        return StatusCode.OK;
    }
}
