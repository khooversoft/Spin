namespace TicketShare.sdk;

public record ChannelInfo
{
    public string ChannelId { get; set; } = null!;
    public string Name { get; init; } = null!;
    public bool HasUnreadMessages { get; init; }
}
