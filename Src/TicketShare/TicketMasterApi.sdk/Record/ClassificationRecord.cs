using TicketMasterApi.sdk.Model.Classification;
using TicketMasterApi.sdk.Model.Event;
using Toolbox.Tools;

namespace TicketMasterApi.sdk;

public record ClassificationRecord
{
    public ClassificationSegmentSubRecord Segement { get; init; } = null!;
    public ClassificationSegmentSubRecord Grene { get; init; } = null!;
    public ClassificationSegmentSubRecord SubGrene { get; init; } = null!;
}

public record ClassificationSegmentSubRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Locale { get; init; }
}


public static class ClassificationRecordExtensions
{
    public static ClassificationSegmentSubRecord ConvertTo(this Event_ClassificationSubModel subject) => new ClassificationSegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
    };
    
    public static ClassificationSegmentSubRecord ConvertTo(this Class_SegmentModel subject) => new ClassificationSegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
        Locale = subject.Locale.NotEmpty(),
    };

    public static ClassificationSegmentSubRecord ConvertTo(this Class_GenreModel subject) => new ClassificationSegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
        Locale = subject.Locale.NotEmpty(),
    };

    public static ClassificationSegmentSubRecord ConvertTo(this Class_SubGenreModel subject) => new ClassificationSegmentSubRecord
    {
        Id = subject.Id.NotEmpty(),
        Name = subject.Name.NotEmpty(),
        Locale = subject.Locale.NotEmpty(),
    };

}