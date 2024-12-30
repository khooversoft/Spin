namespace TicketShare.sdk.Account;

public record FeedItemRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}
