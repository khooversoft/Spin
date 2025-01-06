namespace TicketShare.sdk;

public record ChannelInfo
{
    public string ChannelId { get; set; } = null!;
    public string Name { get; init; } = null!;
    public int UnReadCount { get; init; }
}
