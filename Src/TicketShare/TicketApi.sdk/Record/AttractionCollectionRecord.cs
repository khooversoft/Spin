using System.Collections.Immutable;

namespace TicketApi.sdk;

public record AttractionCollectionRecord
{
    public IReadOnlyList<AttractionRecord> Attractions { get; init; } = Array.Empty<AttractionRecord>();
};

public record AttractionRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public string? Locale { get; init; }
    public AttractionExternalLinksRecord ExternalLinks { get; init; } = new();
    public IReadOnlyList<ImageRecord> Images { get; init; } = Array.Empty<ImageRecord>();
    public IReadOnlyList<AttractionClassificationRecord> Classifications { get; init; } = Array.Empty<AttractionClassificationRecord>();
}

public record AttractionClassificationTypeRecord(string Id, string Name);

public record AttractionClassificationRecord
{
    public AttractionClassificationTypeRecord? Segment { get; init; }
    public AttractionClassificationTypeRecord? Genre { get; init; }
    public AttractionClassificationTypeRecord? SubGenre { get; init; }
    public AttractionClassificationTypeRecord? Type { get; init; }
    public AttractionClassificationTypeRecord? SubType { get; init; }
}

public record AttractionLinkRecord(string Url);

public record AttractionExternalLinksRecord
{
    public IReadOnlyList<AttractionLinkRecord> Twitter { get; init; } = Array.Empty<AttractionLinkRecord>();
    public IReadOnlyList<AttractionLinkRecord> Wiki { get; init; } = Array.Empty<AttractionLinkRecord>();
    public IReadOnlyList<AttractionLinkRecord> Facebook { get; init; } = Array.Empty<AttractionLinkRecord>();
    public IReadOnlyList<AttractionLinkRecord> Instagram { get; init; } = Array.Empty<AttractionLinkRecord>();
    public IReadOnlyList<AttractionLinkRecord> Homepage { get; init; } = Array.Empty<AttractionLinkRecord>();
}


public static class AttractionCollectionRecordExtensions
{
    private static IReadOnlyList<AttractionLinkRecord> _empty = Array.Empty<AttractionLinkRecord>();

    public static AttractionCollectionRecord ConvertTo(this TicketMasterAttraction.AttractionRootModel subject)
    {
        if (subject == null) return new AttractionCollectionRecord();

        var result = subject._embedded.attractions.Select(x => new AttractionRecord
        {
            Id = x.id,
            Name = x.name,
            Url = x.url,
            Locale = x.locale,
            ExternalLinks = x.externalLinks.ConvertTo(),
            Images = x.images.ConvertTo(),
            Classifications = x.classifications.ConvertTo()
        }).ToImmutableArray();

        return new AttractionCollectionRecord { Attractions = result };
    }

    public static IReadOnlyList<ImageRecord> ConvertTo(this IEnumerable<TicketMasterAttraction.Image> subject)
    {
        if (subject == null) return Array.Empty<ImageRecord>();

        var result = subject.Select(x => new ImageRecord
        {
            Url = x.url,
            Ratio = x.ratio,
            Width = x.width,
            Height = x.height,
            Fallback = x.fallback == x.fallback,
        }).ToImmutableArray();

        return result;
    }

    public static IReadOnlyList<AttractionClassificationRecord> ConvertTo(this IEnumerable<TicketMasterAttraction.Classification> subject)
    {
        if (subject == null) return Array.Empty<AttractionClassificationRecord>();

        var result = subject.Select(x => new AttractionClassificationRecord
        {
            Segment = new AttractionClassificationTypeRecord(x.segment.id, x.segment.name),
            Genre = new AttractionClassificationTypeRecord(x.genre.id, x.genre.name),
            SubGenre = new AttractionClassificationTypeRecord(x.subGenre.id, x.subGenre.name),
            Type = x.type is null ? null : new AttractionClassificationTypeRecord(x.type.id, x.type.name),
            SubType = x.subType is null ? null : new AttractionClassificationTypeRecord(x.subType.id, x.subType.name),
        }).ToImmutableArray();
        return result;
    }

    public static AttractionExternalLinksRecord ConvertTo(this TicketMasterAttraction.ExternalLinks subject)
    {
        if (subject == null) return new AttractionExternalLinksRecord();

        return new AttractionExternalLinksRecord
        {
            Twitter = subject.twitter is null ? _empty : subject.twitter.Select(x => new AttractionLinkRecord(x.url)).ToImmutableArray(),
            Wiki = subject.wiki is null ? _empty : subject.wiki.Select(x => new AttractionLinkRecord(x.url)).ToImmutableArray(),
            Facebook = subject.facebook is null ? _empty : subject.facebook.Select(x => new AttractionLinkRecord(x.url)).ToImmutableArray(),
            Instagram = subject.instagram is null ? _empty : subject.instagram.Select(x => new AttractionLinkRecord(x.url)).ToImmutableArray(),
            Homepage = subject.homepage is null ? _empty : subject.homepage.Select(x => new AttractionLinkRecord(x.url)).ToImmutableArray()
        };
    }
}