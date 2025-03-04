using TicketApi.sdk.Model;
using Toolbox.Tools;

namespace TicketApi.sdk;

public record ClassificationRecord
{
    public SegmentSubRecord Segement { get; init; } = null!;
    public SegmentSubRecord Grene { get; init; } = null!;
    public SegmentSubRecord SubGrene { get; init; } = null!;
}

public record SegmentSubRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Locale { get; init; }
}


public static class ClassificationRecordExtensions
{
    public static SegmentSubRecord ConvertTo(this Event_ClassificationSubModel subject) => new SegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
    };

    public static SegmentSubRecord ConvertTo(this Class_SegmentModel subject) => new SegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
        Locale = subject.Locale.NotEmpty(),
    };

    public static SegmentSubRecord ConvertTo(this Class_GenreModel subject) => new SegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
        Locale = subject.Locale.NotEmpty(),
    };

    public static SegmentSubRecord ConvertTo(this Class_SubGenreModel subject) => new SegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
        Locale = subject.Locale.NotEmpty(),
    };

}