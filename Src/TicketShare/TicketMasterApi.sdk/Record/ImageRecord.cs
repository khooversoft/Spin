using TicketMasterApi.sdk.Model.Event;

namespace TicketMasterApi.sdk;

public record ImageRecord
{
    public string? Ratio { get; init; }
    public string? Url { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}

public static class ImageRecordExtensions
{
    public static ImageRecord ConvertTo(this Event_ImageModel subject) => new ImageRecord
    {
        Ratio = subject.Ratio,
        Url = subject.Url,
        Width = subject.Width,
        Height = subject.Height,
    };
}
