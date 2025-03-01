namespace TicketMasterApi.sdk;

public record ImageModel
{
    public string? Ratio { get; init; }
    public string? Url { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}
