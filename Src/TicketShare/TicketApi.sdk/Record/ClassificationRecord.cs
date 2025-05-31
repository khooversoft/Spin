using System.Collections.Immutable;
using System.Diagnostics;
using TicketApi.sdk.TicketMasterClassification;
using Toolbox.Tools;

namespace TicketApi.sdk;

public record ClassificationRecord
{
    public IReadOnlyList<SegmentRecord> Segements { get; init; } = Array.Empty<SegmentRecord>();
}

public record SegmentRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Locale { get; init; }
    public IReadOnlyList<GenreRecord> Genres { get; init; } = Array.Empty<GenreRecord>();
}

public record GenreRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Locale { get; init; }
    public IReadOnlyList<SubGenreRecord> SubGenres { get; init; } = Array.Empty<SubGenreRecord>();
}

public record SubGenreRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Locale { get; init; }
}


public static class ClassificationRecordExtensions
{
    public static ClassificationRecord ConvertTo(this ClassificationRootModel subject)
    {
        return new ClassificationRecord
        {
            Segements = subject._embedded switch
            {
                null => Array.Empty<SegmentRecord>(), // No segments available
                var v => v.classifications.Where(x => x.segment != null).Select(x => x.segment.ConvertTo()).ToImmutableArray(),
            }
        };
    }

    private static SegmentRecord ConvertTo(this Segment subject)
    {
        return new SegmentRecord
        {
            Id = subject.NotNull().id.NotEmpty(),
            Name = subject.name.NotEmpty(),
            Locale = subject.locale.NotEmpty(),
            Genres = subject._embedded switch
            {
                null => Array.Empty<GenreRecord>(),
                var v => v.genres.NotNull().Select(x => x.ConvertTo()).ToImmutableArray(),
            },
        };
    }

    public static GenreRecord ConvertTo(this Genre subject) => new GenreRecord
    {
        Id = subject.NotNull().id.NotEmpty(),
        Name = subject.name.NotEmpty(),
        Locale = subject.locale.NotEmpty(),
        SubGenres = subject._embedded switch
        {
            null => Array.Empty<SubGenreRecord>(),
            var v => v.subgenres.NotNull().Select(x => x.ConvertTo()).ToImmutableArray(),
        },
    };

    public static SubGenreRecord ConvertTo(this Subgenre subject) => new SubGenreRecord
    {
        Id = subject.NotNull().id.NotEmpty(),
        Name = subject.name.NotEmpty(),
        Locale = subject.locale.NotEmpty(),
    };
}