using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class ChannelManager
{
    private readonly AccountClient _accountClient;
    private readonly ChannelClient _channelClient;
    private readonly ILogger<ChannelManager> _logger;

    public ChannelManager(AccountClient accountClient, ChannelClient channelClient, ILogger<ChannelManager> logger)
    {
        _accountClient = accountClient.NotNull();
        _channelClient = channelClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> ProcessMessage(ChannelMessage message, ScopeContext context)
    {
        if (message.Validate().IsError(out var r)) return r.LogStatus(context, nameof(ChannelMessage));

        if (message.IsPrincipalMessage())
        {
            var principalId = IdentityTool.RemoveNodeKeyPrefix(message.ChannelId);
            var user = await _accountClient.GetContext(principalId).Messages.Send([message], context).ConfigureAwait(false);
            if (user.IsError()) return user.LogStatus(context, "Failed to send message to user, principalId={principalId}, message={message}", [principalId, message]);

            context.LogInformation("Send message to principalId={principalId}, message={message}", principalId, message);
            return StatusCode.OK;
        }

        var channel = await _channelClient.GetContext(message.ChannelId, message.FromPrincipalId).AddMessage(message, context).ConfigureAwait(false);
        if (channel.IsError()) return channel.LogStatus(context, "Failed to add message to channel, channelId={channelId}, message={message}", [message.ChannelId, message]);

        context.LogInformation("Message sent to channel, channelId={channelId}, message={message}", message.ChannelId, message);
        return StatusCode.OK;
    }

    public async Task<Option<IReadOnlyList<ChannelMessage>>> GetMessages(string principalId, ScopeContext context)
    {
        var channelsOption = await _channelClient.GetPrincipalMessages(principalId, context);
        IReadOnlyList<ChannelMessage> accountMessages = await _accountClient.GetContext(principalId).Messages.Get(context);

        var list = channelsOption.IsOk() switch
        {
            true => channelsOption.Return(),
            false => [],
        };

        var messageList = list.Concat(accountMessages).ToImmutableArray();
        return messageList;
    }
}
