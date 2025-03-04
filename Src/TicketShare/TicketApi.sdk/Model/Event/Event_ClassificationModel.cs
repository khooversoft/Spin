namespace TicketApi.sdk.Model;

public record Event_ClassificationModel
{
    public Event_ClassificationSubModel? Segment { get; init; }
    public Event_ClassificationSubModel? Genre { get; init; }
    public Event_ClassificationSubModel? SubGenre { get; init; }
}

public record Event_ClassificationSubModel
{
    public string Id { get; init; } = null!;
    public string? Name { get; init; }
}
