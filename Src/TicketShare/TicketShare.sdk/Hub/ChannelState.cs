using System.Diagnostics;
using Toolbox.Tools;

namespace TicketShare.sdk;

[DebuggerDisplay("ChannelId={ChannelId}, UnReadMessages={UnReadMessages}")]
public readonly struct ChannelState
{
    public ChannelState(string channelId, int unReadMessages)
    {
        ChannelId = channelId.NotNull();
        UnReadMessages = unReadMessages;
    }

    public string ChannelId { get; init; }
    public int UnReadMessages { get; init; }

    public override string ToString() => $"channelId={ChannelId}, unReadMessages={UnReadMessages}";
}