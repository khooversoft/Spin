using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Logging;

namespace TicketShare.sdk;

public class MessageSenderHost : ChannelReceiverHost<ChannelMessage>
{
    private readonly ILogger<MessageSenderHost> _logger;
    private readonly ChannelManager _channelManager;

    //private readonly AccountClient _accountClient;
    //private readonly ChannelClient _channelClient;

    public MessageSenderHost(ChannelManager channelManager, Channel<ChannelMessage> messageChannel, ILogger<MessageSenderHost> logger)
        : base(nameof(MessageSenderHost), messageChannel, logger)
    {
        _logger = logger.NotNull();
        _channelManager = channelManager;
        //_accountClient = accountClient;
        //_channelClient = channelClient;
    }

    protected override async Task<Option> ProcessMessage(ChannelMessage message, ScopeContext context)
    {
        if (message.Validate().IsError(out var r)) return r.LogStatus(context, nameof(ChannelMessage));
        _logger.LogInformation("Message received, message={message}", message);

        var result = await _channelManager.ProcessMessage(message, context).ConfigureAwait(false);
        return result;
    }

    //protected override async Task<Option> ProcessMessage(ChannelMessage message, ScopeContext context)
    //{
    //    if (message.Validate().IsError(out var r)) return r.LogStatus(context, nameof(ChannelMessage));

    //    if (message.IsPrincipalMessage())
    //    {
    //        var principalId = IdentityTool.RemoveNodeKeyPrefix(message.ChannelId);
    //        var user = await _accountClient.GetContext(principalId).Messages.Send([message], context).ConfigureAwait(false);
    //        if (user.IsError()) return user.LogStatus(context, "Failed to send message to user, principalId={principalId}, message={message}", [principalId, message]);

    //        context.LogInformation("Send message to principalId={principalId}, message={message}", principalId, message);
    //        return StatusCode.OK;
    //    }

    //    var channel = await _channelClient.GetContext(message.ChannelId, message.FromPrincipalId).AddMessage(message, context).ConfigureAwait(false);
    //    if (channel.IsError()) return channel.LogStatus(context, "Failed to add message to channel, channelId={channelId}, message={message}", [message.ChannelId, message]);

    //    context.LogInformation("Message sent to channel, channelId={channelId}, message={message}", message.ChannelId, message);
    //    return StatusCode.OK;
    //}
}
