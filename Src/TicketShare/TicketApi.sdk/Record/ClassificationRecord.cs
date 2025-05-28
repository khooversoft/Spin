using System.Collections.Immutable;
using System.Diagnostics;
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
    public IReadOnlyList<GenreRecord>? Genres { get; init; }
}

public record GenreRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Locale { get; init; }
    public IReadOnlyList<SubGenreRecord>? SubGenres { get; init; }
}

public record SubGenreRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Locale { get; init; }
}


public static class ClassificationRecordExtensions
{
    public static ClassificationRecord ConvertTo(this Classification.Model.Root subject)
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

    private static SegmentRecord ConvertTo(this Classification.Model.Segment subject)
    {
        if (subject == null) Debugger.Break();
        if (subject.name == null) Debugger.Break();
        if (subject.locale == null) Debugger.Break();
        if (subject._embedded == null) Debugger.Break();

        return new SegmentRecord
        {
            Id = subject.NotNull().id.NotEmpty(),
            Name = subject.name.NotEmpty(),
            Locale = subject.locale.NotEmpty(),
            Genres = subject._embedded switch
            {
                null => Array.Empty<GenreRecord>(),
                var v => v.genres.NotNull("hello").Select(x => x.ConvertTo()).ToImmutableArray(),
            },
        };
    }

    public static GenreRecord ConvertTo(this Classification.Model.Genre subject) => new GenreRecord
    {
        Id = subject.NotNull().id.NotEmpty(),
        Name = subject.name.NotEmpty(),
        Locale = subject.locale.NotEmpty(),
        SubGenres = subject._embedded switch
        {
            null => Array.Empty<SubGenreRecord>(),
            var v => v.subgenres.Select(x => x.ConvertTo()).ToImmutableArray(),
        },
    };

    public static SubGenreRecord ConvertTo(this Classification.Model.Subgenre subject) => new SubGenreRecord
    {
        Id = subject.NotNull().id.NotEmpty(),
        Name = subject.name.NotEmpty(),
        Locale = subject.locale.NotEmpty(),
    };
}