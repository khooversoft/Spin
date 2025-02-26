using System.Collections.Immutable;
using TicketMasterApi.sdk.Model.Event;

namespace TicketMasterApi.sdk;

public record AttractionRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public IReadOnlyList<ImageRecord> Images { get; init; } = Array.Empty<ImageRecord>();
}


public static class AttractionRecordExtensions
{
    public static AttractionRecord ConvertTo(this Event_AttractionModel subject) => new AttractionRecord
    {
        Id = subject.Id,
        Name = subject.Name,
        Url = subject.Url,
        Images = subject.Images.Select(x => x.ConvertTo()).ToImmutableArray(),
    };
}

