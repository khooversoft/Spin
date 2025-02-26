namespace TicketMasterApi.sdk.Model.Event;

public record Event_ImageModel
{
    public string? Ratio { get; init; }
    public string? Url { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}
