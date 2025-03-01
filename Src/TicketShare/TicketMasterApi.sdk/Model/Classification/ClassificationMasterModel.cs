namespace TicketMasterApi.sdk.Model.Classification;

public record ClassificationMasterModel
{
    public Class_Classification_Embedded? _embedded { get; init; }
    public LinkModel? _links { get; init; }
    public PageModel? Page { get; init; }
}

public record Class_Classification_Embedded
{
    public IReadOnlyList<ClassificationModel> Classifications { get; init; } = Array.Empty<ClassificationModel>();
}

public record ClassificationModel
{
    public Class_SegmentModel? Segment { get; set; }
}

public record Class_SegmentModel
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Locale { get; init; }
    public Class_SegmentModel_Embedded? _embedded { get; init; }
}

public record Class_SegmentModel_Embedded
{
    public IReadOnlyList<Class_GenreModel> genres { get; init; } = Array.Empty<Class_GenreModel>();
}

public record Class_GenreModel
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Locale { get; init; }
    public Class_GenreModel_Embedded? _embedded { get; init; }
}


public record Class_GenreModel_Embedded
{
    public IReadOnlyList<Class_SubGenreModel> subgenres { get; init; } = Array.Empty<Class_SubGenreModel>();
}

public record Class_SubGenreModel
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Locale { get; init; }
}

