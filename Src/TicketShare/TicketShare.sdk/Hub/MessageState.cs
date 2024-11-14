using System.Diagnostics;
using Toolbox.Tools;

namespace TicketShare.sdk;

[DebuggerDisplay("Message={Message}, ReadDate={ReadDate}")]
public readonly struct MessageState
{
    public MessageState(ChannelMessageRecord message, DateTime? readDate)
    {
        Message = message.NotNull();
        ReadDate = readDate;
    }

    public ChannelMessageRecord Message { get; }
    public DateTime? ReadDate { get; }

    public override string ToString() => $"message={Message}, readDate={ReadDate}";
}
