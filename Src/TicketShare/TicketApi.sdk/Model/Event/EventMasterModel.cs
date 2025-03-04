namespace TicketApi.sdk.Model;

public record EventMasterModel
{
    public Event_RootEmbedded _embedded { get; init; } = null!;
    public LinkModel _links { get; init; } = null!;
    public PageModel Page { get; init; } = null!;
}

public record Event_RootEmbedded
{
    public IReadOnlyList<Event_EventRecordModel> Events { get; init; } = Array.Empty<Event_EventRecordModel>();
}
