namespace TicketApi.sdk;

public record ImageRecord
{
    public string Url { get; init; } = null!;
    public string Ratio { get; init; } = null!;
    public int? Width { get; init; }
    public int? Height { get; init; }
    public string? Attribution { get; init; }
    public bool Fallback { get; init; }
}
