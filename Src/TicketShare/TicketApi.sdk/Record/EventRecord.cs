using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public record EventCollectionRecord
{
    public IReadOnlyList<EventRecord> Events { get; init; } = Array.Empty<EventRecord>();
}

public record EventRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public DateTime? LocalDateTime { get; init; }
    public string? Timezone { get; init; }
    public string? EventUrl { get; init; }
    public EventClassificationRecord ClassificationRecord { get; init; } = null!;
    public EventVenueRecord Venue { get; init; } = null!;
    public IReadOnlyList<EventAttractionRecord> Attractions { get; init; } = Array.Empty<EventAttractionRecord>();
}

public record EventClassificationTypeRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
}

public record EventClassificationRecord
{
    public EventClassificationTypeRecord? Segment { get; init; }
    public EventClassificationTypeRecord? Genre { get; init; }
    public EventClassificationTypeRecord? SubGenre { get; init; }
    public EventClassificationTypeRecord? Type { get; init; }
    public EventClassificationTypeRecord? SubType { get; init; }
}

public record EventVenueRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public string? Locale { get; init; }
}

public record EventAttractionRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public string? Locale { get; init; }
}


public static class EventCollectionModelExtensions
{
    public static EventCollectionRecord ConvertTo(this TicketMasterEvent.EventRootModel model)
    {
        var result = new EventCollectionRecord
        {
            Events = model._embedded.events.Select(x => x.ConvertTo()).ToImmutableArray(),
        };

        return result;
    }

    private static EventRecord ConvertTo(this TicketMasterEvent.EventModel x)
    {
        var result = new EventRecord
        {
            Id = x.id,
            Name = x.name,
            LocalDateTime = x.dates?.start?.dateTime,
            Timezone = x.dates?.timezone,
            EventUrl = x.url,
            ClassificationRecord = x.classifications.ConvertTo(),
            Venue = x._embedded.venues.ConvertTo(),
            Attractions = x._embedded.attractions.ConvertTo(),
        };

        return result;
    }

    private static EventClassificationRecord ConvertTo(this IEnumerable<TicketMasterEvent.ClassificationModel> subject)
    {
        var item = subject.NotNull().First();

        var result = new EventClassificationRecord
        {
            Segment = new EventClassificationTypeRecord { Id = item.segment.id, Name = item.segment.name },
            Genre = item.genre is null ? null : new EventClassificationTypeRecord { Id = item.genre.id, Name = item.genre.name },
            SubGenre = item.subGenre is null ? null : new EventClassificationTypeRecord { Id = item.subGenre.id, Name = item.subGenre.name },
            Type = item.type is null ? null : new EventClassificationTypeRecord { Id = item.type.id, Name = item.type.name },
            SubType = item.subType is null ? null : new EventClassificationTypeRecord { Id = item.subType.id, Name = item.subType.name },
        };

        return result;
    }

    private static EventVenueRecord ConvertTo(this IEnumerable<TicketMasterEvent.VenueModel> subject)
    {
        var item = subject.NotNull().First();

        var result = new EventVenueRecord
        {
            Id = item.id,
            Name = item.name,
            Url = item.url,
            Locale = item.locale
        };

        return result;
    }

    private static IReadOnlyList<EventAttractionRecord> ConvertTo(this IEnumerable<TicketMasterEvent.AttractionModel> subject)
    {
        var result = subject.NotNull().Select(x => new EventAttractionRecord
        {
            Id = x.id,
            Name = x.name,
            Url = x.url,
            Locale = x.locale
        }).ToImmutableArray();

        return result;
    }
}